import { HubConnection, HubConnectionBuilder, LogLevel, HubConnectionState } from '@microsoft/signalr'
import type {
  ConnectionState,
  SignalREvents,
  ReconnectionConfig,
  TypingIndicator,
  UserPresence,
  SignalRMessage,
  CallOffer,
  CallAnswer,
  CallIceCandidate,
  CallRejection,
  CallTermination,
  CallStateUpdate
} from '../types/signalr'
import { CallType } from '../types/signalr'
import { ref } from 'vue'

class SignalRService {
  private connection: HubConnection | null = null
  private connectionState = ref<ConnectionState>({
    status: 'disconnected',
    reconnectAttempts: 0
  })
  
  private reconnectionConfig: ReconnectionConfig = {
    maxRetries: 10,
    baseDelay: 1000,
    maxDelay: 30000,
    jitter: true
  }
  
  private reconnectionTimer: number | null = null
  private typingTimeouts = new Map<string, number>()
  
  // Event handlers
  private eventHandlers = new Map<keyof SignalREvents, Set<Function>>()

  constructor() {
    this.setupConnection()
  }

  private setupConnection(): void {
    this.connection = new HubConnectionBuilder()
      .withUrl('https://thrume-api.onrender.com/chathub', {
        withCredentials: true // Use existing cookie authentication
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          return this.calculateRetryDelay(retryContext.previousRetryCount)
        }
      })
      .configureLogging(LogLevel.Information)
      .build()

    this.setupEventHandlers()
    this.setupConnectionEventHandlers()
  }

  private setupEventHandlers(): void {
    if (!this.connection) return

    // Server to client events
    this.connection.on('ReceiveMessage', (message: SignalRMessage) => {
      console.log('SignalR received message:', message)
      this.emit('ReceiveMessage', message)
    })

    this.connection.on('TypingIndicator', (conversationId: string, userId: string, isTyping: boolean) => {
      const indicator: TypingIndicator = {
        conversationId,
        userId,
        isTyping,
        timestamp: new Date()
      }
      this.emit('ReceiveTypingIndicator', indicator)
    })

    this.connection.on('UserJoined', (conversationId: string, userId: string) => {
      const presence: UserPresence = {
        userId,
        isOnline: true,
        lastSeen: new Date()
      }
      this.emit('UserJoined', presence)
    })

    this.connection.on('UserLeft', (conversationId: string, userId: string) => {
      const presence: UserPresence = {
        userId,
        isOnline: false,
        lastSeen: new Date()
      }
      this.emit('UserLeft', presence)
    })

    this.connection.on('PresenceUpdated', (userId: string, isOnline: boolean) => {
      const presence: UserPresence = {
        userId,
        isOnline,
        lastSeen: new Date()
      }
      this.emit('UserJoined', presence)
    })

    this.connection.on('MessageRead', (messageId: string, userId: string) => {
      this.emit('MessageRead', messageId, userId)
    })

    this.connection.on('Error', (message: string) => {
      console.error('SignalR Error:', message)
    })

    this.connection.on('CallError', (message: string) => {
      console.error('🚨 [SignalR] CallError received from backend:', message)
    })

    // Call-related events
    this.connection.on('IncomingCall', (callOffer: CallOffer) => {
      console.log('📞 [SignalR] Received IncomingCall:', callOffer)
      this.emit('IncomingCall', callOffer)
    })

    this.connection.on('CallRinging', (callId: string) => {
      console.log('📞 [SignalR] Received CallRinging:', callId)
      // This event indicates the call is ringing on the callee's end
      // We could emit this to update UI state if needed
    })

    this.connection.on('CallAccepted', (callId: string) => {
      console.log('📞 [SignalR] Received CallAccepted:', callId)
      this.emit('CallAccepted', callId)
    })

    this.connection.on('CallRejected', (callRejection: CallRejection) => {
      console.log('SignalR call rejected:', callRejection)
      this.emit('CallRejected', callRejection)
    })

    this.connection.on('CallEnded', (callTermination: CallTermination) => {
      console.log('SignalR call ended:', callTermination)
      this.emit('CallEnded', callTermination)
    })

    this.connection.on('ReceiveOffer', (callOffer: CallOffer) => {
      console.log('SignalR received offer:', callOffer)
      this.emit('ReceiveOffer', callOffer)
    })

    this.connection.on('ReceiveAnswer', (callAnswer: CallAnswer) => {
      console.log('SignalR received answer:', callAnswer)
      this.emit('ReceiveAnswer', callAnswer)
    })

    this.connection.on('ReceiveIceCandidate', (iceCandidate: CallIceCandidate) => {
      console.log('SignalR received ICE candidate:', iceCandidate)
      this.emit('ReceiveIceCandidate', iceCandidate)
    })

    this.connection.on('CallStateChanged', (callStateUpdate: CallStateUpdate) => {
      console.log('SignalR call state changed:', callStateUpdate)
      this.emit('CallStateChanged', callStateUpdate)
    })
  }

  private setupConnectionEventHandlers(): void {
    if (!this.connection) return

    this.connection.onclose((error) => {
      console.error('SignalR connection closed:', error)
      this.updateConnectionState({
        status: 'disconnected',
        error: error?.message
      })
    })

    this.connection.onreconnecting((error) => {
      console.warn('SignalR reconnecting:', error)
      this.updateConnectionState({
        status: 'reconnecting',
        error: error?.message
      })
    })

    this.connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected:', connectionId)
      this.updateConnectionState({
        status: 'connected',
        reconnectAttempts: 0,
        error: undefined,
        lastConnected: new Date()
      })
    })
  }

  private calculateRetryDelay(retryCount: number): number {
    const delay = Math.min(
      this.reconnectionConfig.baseDelay * Math.pow(2, retryCount),
      this.reconnectionConfig.maxDelay
    )
    
    if (this.reconnectionConfig.jitter) {
      return delay + Math.random() * 1000
    }
    
    return delay
  }

  private updateConnectionState(updates: Partial<ConnectionState>): void {
    this.connectionState.value = {
      ...this.connectionState.value,
      ...updates
    }
  }

  private emit<K extends keyof SignalREvents>(
    event: K,
    ...args: Parameters<SignalREvents[K]>
  ): void {
    const handlers = this.eventHandlers.get(event)
    if (handlers) {
      handlers.forEach(handler => {
        try {
          handler(...args)
        } catch (error) {
          console.error(`Error in SignalR event handler for ${String(event)}:`, error)
        }
      })
    }
  }

  // Public API
  async connect(): Promise<void> {
    if (!this.connection) {
      this.setupConnection()
    }

    if (this.connection!.state === HubConnectionState.Connected) {
      return
    }

    this.updateConnectionState({ status: 'connecting' })

    try {
      await this.connection!.start()
      this.updateConnectionState({
        status: 'connected',
        reconnectAttempts: 0,
        error: undefined,
        lastConnected: new Date()
      })
      console.log('SignalR connected successfully')
    } catch (error) {
      console.error('SignalR connection failed:', error)
      this.updateConnectionState({
        status: 'disconnected',
        error: error instanceof Error ? error.message : 'Unknown error'
      })
      this.scheduleReconnection()
      throw error
    }
  }

  async disconnect(): Promise<void> {
    if (this.reconnectionTimer) {
      clearTimeout(this.reconnectionTimer)
      this.reconnectionTimer = null
    }

    if (this.connection && this.connection.state === HubConnectionState.Connected) {
      await this.connection.stop()
    }

    this.updateConnectionState({
      status: 'disconnected',
      reconnectAttempts: 0,
      error: undefined
    })
  }

  private scheduleReconnection(): void {
    if (this.connectionState.value.reconnectAttempts >= this.reconnectionConfig.maxRetries) {
      console.error('Max reconnection attempts reached')
      return
    }

    const delay = this.calculateRetryDelay(this.connectionState.value.reconnectAttempts)
    
    this.reconnectionTimer = setTimeout(async () => {
      this.updateConnectionState({
        reconnectAttempts: this.connectionState.value.reconnectAttempts + 1
      })
      
      try {
        await this.connect()
      } catch (error) {
        console.error('Reconnection attempt failed:', error)
      }
    }, delay)
  }

  // Event subscription methods
  on<K extends keyof SignalREvents>(
    event: K,
    handler: SignalREvents[K]
  ): () => void {
    if (!this.eventHandlers.has(event)) {
      this.eventHandlers.set(event, new Set())
    }
    
    this.eventHandlers.get(event)!.add(handler)
    
    // Return unsubscribe function
    return () => {
      const handlers = this.eventHandlers.get(event)
      if (handlers) {
        handlers.delete(handler)
      }
    }
  }

  off<K extends keyof SignalREvents>(
    event: K,
    handler: SignalREvents[K]
  ): void {
    const handlers = this.eventHandlers.get(event)
    if (handlers) {
      handlers.delete(handler)
    }
  }

  // Server method calls
  async joinConversation(conversationId: string): Promise<void> {
    if (this.connection?.state !== HubConnectionState.Connected) {
      console.warn('SignalR: Cannot join conversation - not connected')
      return // Silently fail if not connected
    }

    
    await this.connection.invoke('JoinConversationAsync', conversationId)
    
  }

  async leaveConversation(conversationId: string): Promise<void> {
    if (this.connection?.state !== HubConnectionState.Connected) {
      return // Silently fail if not connected
    }

    await this.connection.invoke('LeaveConversationAsync', conversationId)
  }

  async startTyping(conversationId: string): Promise<void> {
    if (this.connection?.state !== HubConnectionState.Connected) {
      return
    }

    // Clear existing timeout for this conversation
    const existingTimeout = this.typingTimeouts.get(conversationId)
    if (existingTimeout) {
      clearTimeout(existingTimeout)
    }

    await this.connection.invoke('SendTypingIndicatorAsync', conversationId)

    // Auto-stop typing after 3 seconds
    const timeout = setTimeout(() => {
      this.stopTyping(conversationId)
      this.typingTimeouts.delete(conversationId)
    }, 3000)

    this.typingTimeouts.set(conversationId, timeout)
  }

  async stopTyping(conversationId: string): Promise<void> {
    if (this.connection?.state !== HubConnectionState.Connected) {
      return
    }

    // Clear timeout
    const timeout = this.typingTimeouts.get(conversationId)
    if (timeout) {
      clearTimeout(timeout)
      this.typingTimeouts.delete(conversationId)
    }

    await this.connection.invoke('StopTypingIndicatorAsync', conversationId)
  }

  async markMessageAsRead(messageId: string): Promise<void> {
    if (this.connection?.state !== HubConnectionState.Connected) {
      return
    }

    await this.connection.invoke('MarkMessageAsRead', messageId)
  }

  // Call-related server methods
  async initiateCall(calleeId: string, callType: CallType, conversationId: string): Promise<void> {
    console.log('📡 [SignalR] Attempting to initiate call:', { calleeId, callType, conversationId })
    
    if (this.connection?.state !== HubConnectionState.Connected) {
      console.error('❌ [SignalR] Cannot initiate call - not connected, state:', this.connection?.state)
      return
    }

    console.log('📡 [SignalR] Connection state is connected, invoking InitiateCallAsync...')
    try {
      // FIXED: Now passing conversationId to backend
      await this.connection.invoke('InitiateCallAsync', calleeId, callType, conversationId)
      console.log('✅ [SignalR] InitiateCallAsync invoked successfully')
    } catch (error) {
      console.error('❌ [SignalR] Error invoking InitiateCallAsync:', error)
      throw error
    }
  }

  async acceptCall(callId: string): Promise<void> {
    if (this.connection?.state !== HubConnectionState.Connected) {
      console.warn('SignalR: Cannot accept call - not connected')
      return
    }

    await this.connection.invoke('AcceptCallAsync', callId)
  }

  async rejectCall(callId: string, reason?: string): Promise<void> {
    if (this.connection?.state !== HubConnectionState.Connected) {
      console.warn('SignalR: Cannot reject call - not connected')
      return
    }

    await this.connection.invoke('RejectCallAsync', callId, reason)
  }

  async endCall(callId: string): Promise<void> {
    if (this.connection?.state !== HubConnectionState.Connected) {
      console.warn('SignalR: Cannot end call - not connected')
      return
    }

    await this.connection.invoke('EndCallAsync', callId)
  }

  async sendOffer(callId: string, offer: RTCSessionDescriptionInit): Promise<void> {
    if (this.connection?.state !== HubConnectionState.Connected) {
      console.warn('SignalR: Cannot send offer - not connected')
      return
    }

    await this.connection.invoke('SendOfferAsync', callId, JSON.stringify(offer))
  }

  async sendAnswer(callId: string, answer: RTCSessionDescriptionInit): Promise<void> {
    if (this.connection?.state !== HubConnectionState.Connected) {
      console.warn('SignalR: Cannot send answer - not connected')
      return
    }

    await this.connection.invoke('SendAnswerAsync', callId, JSON.stringify(answer))
  }

  async sendIceCandidate(callId: string, candidate: RTCIceCandidateInit): Promise<void> {
    if (this.connection?.state !== HubConnectionState.Connected) {
      console.warn('SignalR: Cannot send ICE candidate - not connected')
      return
    }

    await this.connection.invoke('SendIceCandidateAsync', callId, JSON.stringify(candidate))
  }

  // Getters
  get state(): ConnectionState {
    return this.connectionState.value
  }

  get isConnected(): boolean {
    return this.connectionState.value.status === 'connected'
  }

  get isConnecting(): boolean {
    return this.connectionState.value.status === 'connecting' || 
           this.connectionState.value.status === 'reconnecting'
  }
}

// Export singleton instance
export const signalRService = new SignalRService()
export default signalRService