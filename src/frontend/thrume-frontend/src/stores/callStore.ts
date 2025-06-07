import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { CallState, CallType, type Call, type MediaState, type CallOffer } from '../types/signalr'
import { webRTCService } from '../services/webRTCService'
import { permissionService } from '../services/permissionService'
import { signalRService } from '../services/signalRService'
import { useAuthStore } from './authStore'
import { useAccountStore } from './accountStore'

export interface CallStoreState {
  activeCall: Call | null
  incomingCall: Call | null
  callHistory: Call[]
  localStream: MediaStream | null
  remoteStream: MediaStream | null
  mediaState: MediaState
  callError: string | null
  isInitiating: boolean
}

export const useCallStore = defineStore('call', () => {
  // State
  const activeCall = ref<Call | null>(null)
  const incomingCall = ref<Call | null>(null)
  const callHistory = ref<Call[]>([])
  const localStream = ref<MediaStream | null>(null)
  const remoteStream = ref<MediaStream | null>(null)
  const mediaState = ref<MediaState>({
    isMuted: false,
    isCameraOn: false,
    isScreenSharing: false
  })
  const callError = ref<string | null>(null)
  const isInitiating = ref<boolean>(false)

  // Computed
  const isCallActive = computed(() => activeCall.value !== null)
  const hasIncomingCall = computed(() => incomingCall.value !== null)
  const callStatus = computed(() => activeCall.value?.state || CallState.Idle)

  // Initialize SignalR event handlers
  const initializeSignalRHandlers = () => {
    // Handle incoming call
    signalRService.on('IncomingCall', handleIncomingCall)
    
    // Handle call accepted
    signalRService.on('CallAccepted', handleCallAccepted)
    
    // Handle call rejected
    signalRService.on('CallRejected', handleCallRejected)
    
    // Handle call ended
    signalRService.on('CallEnded', handleCallEnded)
    
    // Handle WebRTC signaling
    signalRService.on('ReceiveOffer', handleReceiveOffer)
    signalRService.on('ReceiveAnswer', handleReceiveAnswer)
    signalRService.on('ReceiveIceCandidate', handleReceiveIceCandidate)
    
    // Handle call state changes
    signalRService.on('CallStateChanged', handleCallStateChanged)
  }

  // Initialize WebRTC event handlers
  const initializeWebRTCHandlers = () => {
    webRTCService.onLocalStream((stream) => {
      localStream.value = stream
      updateMediaState()
    })

    webRTCService.onRemoteStream((stream) => {
      remoteStream.value = stream
    })

    webRTCService.onIceCandidate(async (candidate) => {
      if (activeCall.value) {
        await signalRService.sendIceCandidate(activeCall.value.id, candidate)
      }
    })

    webRTCService.onConnectionStateChange((state) => {
      console.log('WebRTC connection state:', state)
      if (state === 'failed' || state === 'disconnected') {
        handleConnectionFailure()
      }
    })
  }

  // Event handlers
  const handleIncomingCall = (callOffer: CallOffer) => {
    console.log('📞 [CallStore] Received incoming call:', callOffer)
    const accountStore = useAccountStore()
    
    // Don't accept calls if already in a call
    if (activeCall.value) {
      console.log('❌ [CallStore] Rejecting incoming call - already in a call')
      signalRService.rejectCall(callOffer.callId, 'User busy')
      return
    }

    console.log('✅ [CallStore] Creating incoming call object')
    incomingCall.value = {
      id: callOffer.callId,
      callerId: callOffer.callerId,
      calleeId: accountStore.accountId || '',
      callerUsername: callOffer.callerUsername,
      callType: callOffer.callType,
      state: CallState.Ringing,
      conversationId: callOffer.conversationId,
      startedAt: new Date(callOffer.timestamp),
      mediaState: {
        isMuted: false,
        isCameraOn: callOffer.callType === CallType.Video,
        isScreenSharing: false
      }
    }
    console.log('📞 [CallStore] Incoming call set:', incomingCall.value)
  }

  const handleCallAccepted = async (callId: string) => {
    if (activeCall.value && activeCall.value.id === callId) {
      activeCall.value.state = CallState.Connecting
      
      try {
        // Create and send offer
        const offer = await webRTCService.createOffer()
        await signalRService.sendOffer(callId, offer)
      } catch (error) {
        console.error('Error creating offer:', error)
        setCallError('Failed to establish connection')
        await endCall()
      }
    }
  }

  const handleCallRejected = (rejection: any) => {
    if (activeCall.value && activeCall.value.id === rejection.callId) {
      setCallError('Call was rejected')
      cleanupCall()
    }
  }

  const handleCallEnded = (termination: any) => {
    if (activeCall.value && activeCall.value.id === termination.callId) {
      cleanupCall()
    }
    if (incomingCall.value && incomingCall.value.id === termination.callId) {
      incomingCall.value = null
    }
  }

  const handleReceiveOffer = async (callOffer: CallOffer) => {
    if (!activeCall.value || activeCall.value.id !== callOffer.callId) {
      return
    }

    try {
      const offer = JSON.parse(callOffer.offer as any) as RTCSessionDescriptionInit
      const answer = await webRTCService.handleOffer(offer)
      await signalRService.sendAnswer(callOffer.callId, answer)
      activeCall.value.state = CallState.Connected
    } catch (error) {
      console.error('Error handling offer:', error)
      setCallError('Failed to handle incoming call')
      await endCall()
    }
  }

  const handleReceiveAnswer = async (callAnswer: any) => {
    if (!activeCall.value || activeCall.value.id !== callAnswer.callId) {
      return
    }

    try {
      const answer = JSON.parse(callAnswer.answer) as RTCSessionDescriptionInit
      await webRTCService.handleAnswer(answer)
      activeCall.value.state = CallState.Connected
    } catch (error) {
      console.error('Error handling answer:', error)
      setCallError('Failed to establish connection')
      await endCall()
    }
  }

  const handleReceiveIceCandidate = async (iceCandidate: any) => {
    if (!activeCall.value || activeCall.value.id !== iceCandidate.callId) {
      return
    }

    try {
      const candidate = JSON.parse(iceCandidate.candidate) as RTCIceCandidateInit
      await webRTCService.addIceCandidate(candidate)
    } catch (error) {
      console.error('Error adding ICE candidate:', error)
    }
  }

  const handleCallStateChanged = (stateUpdate: any) => {
    if (activeCall.value && activeCall.value.id === stateUpdate.callId) {
      activeCall.value.state = stateUpdate.state
    }
  }

  const handleConnectionFailure = () => {
    setCallError('Connection failed')
    endCall()
  }

  // Actions
  const initiateCall = async (calleeId: string, callType: CallType, conversationId: string) => {
    const accountStore = useAccountStore()
    
    console.log('🔵 [CallStore] Initiating call:', { calleeId, callType, conversationId })
    
    if (!accountStore.isLoggedIn) {
      console.error('❌ [CallStore] User not authenticated')
      setCallError('User not authenticated')
      return
    }

    if (activeCall.value) {
      console.error('❌ [CallStore] Already in a call')
      setCallError('Already in a call')
      return
    }

    isInitiating.value = true
    callError.value = null

    try {
      console.log('🔍 [CallStore] Checking permissions for:', callType)
      // Check permissions first
      const hasPermissions = await checkAndRequestPermissions(callType)
      if (!hasPermissions) {
        console.error('❌ [CallStore] Permissions denied')
        setCallError('Permissions denied')
        return
      }
      console.log('✅ [CallStore] Permissions granted')

      // Create call object
      const call: Call = {
        id: '', // Will be set by server
        callerId: accountStore.accountId || '',
        calleeId,
        callerUsername: accountStore.accountUsername || '',
        callType,
        state: CallState.Initiating,
        conversationId,
        startedAt: new Date(),
        mediaState: {
          isMuted: false,
          isCameraOn: callType === CallType.Video,
          isScreenSharing: false
        }
      }

      activeCall.value = call
      console.log('📞 [CallStore] Created call object:', call)

      console.log('🔧 [CallStore] Initializing WebRTC...')
      // Initialize WebRTC
      await webRTCService.initializePeerConnection('temp-call-id')
      
      // Get user media
      await webRTCService.getUserMedia(callType)
      console.log('✅ [CallStore] WebRTC initialized and user media obtained')
      
      console.log('📡 [CallStore] Sending call initiation to server...')
      // Send call initiation to server
      await signalRService.initiateCall(calleeId, callType, conversationId)
      
      activeCall.value.state = CallState.Ringing
      console.log('✅ [CallStore] Call initiation sent, state set to Ringing')

    } catch (error) {
      console.error('❌ [CallStore] Error initiating call:', error)
      setCallError('Failed to initiate call')
      cleanupCall()
    } finally {
      isInitiating.value = false
    }
  }

  const acceptCall = async () => {
    if (!incomingCall.value) {
      return
    }

    try {
      // Check permissions
      const hasPermissions = await checkAndRequestPermissions(incomingCall.value.callType)
      if (!hasPermissions) {
        await rejectCall('Permissions denied')
        return
      }

      // Move incoming call to active call
      activeCall.value = { ...incomingCall.value }
      incomingCall.value = null
      activeCall.value.state = CallState.Connecting

      // Initialize WebRTC
      await webRTCService.initializePeerConnection(activeCall.value.id)
      
      // Get user media
      await webRTCService.getUserMedia(activeCall.value.callType)

      // Accept call through SignalR
      await signalRService.acceptCall(activeCall.value.id)

    } catch (error) {
      console.error('Error accepting call:', error)
      setCallError('Failed to accept call')
      cleanupCall()
    }
  }

  const rejectCall = async (reason?: string) => {
    if (!incomingCall.value) {
      return
    }

    try {
      await signalRService.rejectCall(incomingCall.value.id, reason)
      incomingCall.value = null
    } catch (error) {
      console.error('Error rejecting call:', error)
    }
  }

  const endCall = async () => {
    if (!activeCall.value) {
      return
    }

    try {
      await signalRService.endCall(activeCall.value.id)
      cleanupCall()
    } catch (error) {
      console.error('Error ending call:', error)
      cleanupCall() // Cleanup anyway
    }
  }

  const toggleMute = async () => {
    if (!activeCall.value) return

    try {
      const isMuted = webRTCService.toggleMute()
      mediaState.value.isMuted = isMuted
      activeCall.value.mediaState.isMuted = isMuted
    } catch (error) {
      console.error('Error toggling mute:', error)
      setCallError('Failed to toggle mute')
    }
  }

  const toggleCamera = async () => {
    if (!activeCall.value) return

    try {
      const isCameraOn = webRTCService.toggleCamera()
      mediaState.value.isCameraOn = isCameraOn
      activeCall.value.mediaState.isCameraOn = isCameraOn
    } catch (error) {
      console.error('Error toggling camera:', error)
      setCallError('Failed to toggle camera')
    }
  }

  const toggleScreenShare = async () => {
    if (!activeCall.value) return

    try {
      const isScreenSharing = await webRTCService.toggleScreenShare()
      mediaState.value.isScreenSharing = isScreenSharing
      activeCall.value.mediaState.isScreenSharing = isScreenSharing
    } catch (error) {
      console.error('Error toggling screen share:', error)
      setCallError('Failed to toggle screen share')
    }
  }

  // Helper functions
  const checkAndRequestPermissions = async (callType: CallType): Promise<boolean> => {
    try {
      if (callType === CallType.Video) {
        const result = await permissionService.requestAVPermissions()
        return result.granted
      } else {
        const result = await permissionService.requestMicrophonePermission()
        return result.granted
      }
    } catch (error) {
      console.error('Permission error:', error)
      return false
    }
  }

  const updateMediaState = () => {
    mediaState.value = webRTCService.getMediaState()
    if (activeCall.value) {
      activeCall.value.mediaState = { ...mediaState.value }
    }
  }

  const setCallError = (error: string) => {
    callError.value = error
    console.error('Call error:', error)
  }

  const cleanupCall = () => {
    if (activeCall.value) {
      // Add to call history
      const endedCall = { ...activeCall.value }
      endedCall.endedAt = new Date()
      endedCall.state = CallState.Ended
      callHistory.value.unshift(endedCall)
      
      // Keep only last 50 calls
      if (callHistory.value.length > 50) {
        callHistory.value = callHistory.value.slice(0, 50)
      }
    }

    // Cleanup WebRTC
    webRTCService.endCall()

    // Reset state
    activeCall.value = null
    localStream.value = null
    remoteStream.value = null
    mediaState.value = {
      isMuted: false,
      isCameraOn: false,
      isScreenSharing: false
    }
    callError.value = null
    isInitiating.value = false
  }

  const clearError = () => {
    callError.value = null
  }

  const clearIncomingCall = () => {
    incomingCall.value = null
  }

  // Initialize handlers when store is created
  initializeSignalRHandlers()
  initializeWebRTCHandlers()

  return {
    // State
    activeCall,
    incomingCall,
    callHistory,
    localStream,
    remoteStream,
    mediaState,
    callError,
    isInitiating,
    
    // Computed
    isCallActive,
    hasIncomingCall,
    callStatus,
    
    // Actions
    initiateCall,
    acceptCall,
    rejectCall,
    endCall,
    toggleMute,
    toggleCamera,
    toggleScreenShare,
    clearError,
    clearIncomingCall
  }
})