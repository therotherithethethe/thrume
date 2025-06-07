import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Message, Conversation } from '../types'
import type { TypingIndicator, UserPresence, SignalRMessage } from '../types/signalr'
import * as messageService from '../services/messageService'
import signalRService from '../services/signalRService'

export const useMessageStore = defineStore('message', () => {
  // State
  const conversations = ref<Conversation[]>([])
  const messages = ref<Message[]>([])
  const typingIndicators = ref<Map<string, TypingIndicator[]>>(new Map())
  const userPresences = ref<Map<string, UserPresence>>(new Map())
  const activeConversationId = ref<string | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Computed
  const activeConversation = computed(() => {
    if (!activeConversationId.value) return null
    return conversations.value.find(c => c.id.value === activeConversationId.value) || null
  })

  const activeMessages = computed(() => {
    if (!activeConversationId.value) return []
    return messages.value
      .filter(m => m.conversationId.value === activeConversationId.value)
      .sort((a, b) => new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime())
  })

  const activeTypingUsers = computed(() => {
    if (!activeConversationId.value) return []
    return typingIndicators.value.get(activeConversationId.value) || []
  })

  const onlineUsers = computed(() => {
    return Array.from(userPresences.value.values()).filter(p => p.isOnline)
  })

  // SignalR event handlers
  let unsubscribeFunctions: (() => void)[] = []

  const setupSignalRListeners = () => {
    // Clean up existing listeners
    unsubscribeFunctions.forEach(fn => fn())
    unsubscribeFunctions = []

    // Message received
    unsubscribeFunctions.push(
      signalRService.on('ReceiveMessage', (signalRMessage: SignalRMessage) => {
        const message: Message = {
          id: { value: (signalRMessage as any).id?.value || (signalRMessage as any).id },
          conversationId: { value: (signalRMessage as any).conversationId?.value || (signalRMessage as any).conversationId },
          senderId: { value: (signalRMessage as any).senderId?.value || (signalRMessage as any).senderId },
          userName: (signalRMessage as any).senderUserName,
          pictureUrl: (signalRMessage as any).senderPictureUrl,
          content: (signalRMessage as any).content,
          sentAt: (signalRMessage as any).sentAt
        }
        addMessage(message)
      })
    )

    // Typing indicators
    unsubscribeFunctions.push(
      signalRService.on('ReceiveTypingIndicator', (indicator: TypingIndicator) => {
        updateTypingIndicator(indicator)
      })
    )

    // User presence
    unsubscribeFunctions.push(
      signalRService.on('UserJoined', (presence: UserPresence) => {
        userPresences.value.set(presence.userId, presence)
      })
    )

    unsubscribeFunctions.push(
      signalRService.on('UserLeft', (presence: UserPresence) => {
        userPresences.value.set(presence.userId, presence)
      })
    )

    // Message read status
    unsubscribeFunctions.push(
      signalRService.on('MessageRead', (messageId: string, userId: string) => {
        const message = messages.value.find(m => m.id.value === messageId)
        if (message) {
          console.log(`Message ${messageId} read by ${userId}`)
        }
      })
    )
  }

  // Actions
  const fetchConversations = async () => {
    try {
      isLoading.value = true
      error.value = null
      const conversationList = await messageService.getConversations()
      conversations.value = conversationList
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch conversations'
      console.error('Error fetching conversations:', err)
    } finally {
      isLoading.value = false
    }
  }

  const fetchMessages = async (conversationId: string) => {
    try {
      isLoading.value = true
      error.value = null
      const messageList = await messageService.getMessages(conversationId)
      
      // Replace messages for this conversation
      messages.value = messages.value.filter(m => m.conversationId.value !== conversationId)
      messages.value.push(...messageList)
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch messages'
      console.error('Error fetching messages:', err)
    } finally {
      isLoading.value = false
    }
  }

  const sendMessage = async (conversationId: string, content: string) => {
    try {
      error.value = null
      
      // Always use REST API for sending messages
      // The backend will broadcast via SignalR after successful send
      const response = await messageService.sendMessage(conversationId, content)
      
      // Note: We don't need to manually add the message here
      // because the backend broadcasts it via SignalR and our listener will handle it
      // However, if SignalR is not connected, we should add it manually as fallback
      if (!signalRService.isConnected) {
        const message: Message = {
          id: response.id,
          conversationId: response.conversationId,
          senderId: response.senderId,
          userName: 'You', // Current user
          pictureUrl: null,
          content: response.content,
          sentAt: response.timestamp
        }
        addMessage(message)
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to send message'
      console.error('Error sending message:', err)
      throw err
    }
  }

  const startConversation = async (otherUserName: string) => {
    try {
      isLoading.value = true
      error.value = null
      const conversation = await messageService.startConversation(otherUserName)
      
      // Add to conversations if not already present
      const existingIndex = conversations.value.findIndex(c => c.id.value === conversation.id.value)
      if (existingIndex >= 0) {
        conversations.value[existingIndex] = conversation
      } else {
        conversations.value.unshift(conversation)
      }
      
      return conversation
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to start conversation'
      console.error('Error starting conversation:', err)
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const setActiveConversation = async (conversationId: string) => {
    // Leave previous conversation
    if (activeConversationId.value && signalRService.isConnected) {
      await signalRService.leaveConversation(activeConversationId.value)
    }

    activeConversationId.value = conversationId
    
    // Join new conversation
    if (signalRService.isConnected) {
      await signalRService.joinConversation(conversationId)
    }

    // Fetch messages for this conversation
    await fetchMessages(conversationId)
  }

  const clearActiveConversation = async () => {
    if (activeConversationId.value && signalRService.isConnected) {
      await signalRService.leaveConversation(activeConversationId.value)
    }
    activeConversationId.value = null
  }

  const startTyping = async (conversationId: string) => {
    if (signalRService.isConnected) {
      await signalRService.startTyping(conversationId)
    }
  }

  const stopTyping = async (conversationId: string) => {
    if (signalRService.isConnected) {
      await signalRService.stopTyping(conversationId)
    }
  }

  const markMessageAsRead = async (messageId: string) => {
    if (signalRService.isConnected) {
      await signalRService.markMessageAsRead(messageId)
    }
  }

  // Helper functions
  const addMessage = (message: Message) => {
    console.log('addMessage called with:', message)
    console.log('Current activeConversationId:', activeConversationId.value)
    console.log('Message conversationId:', message.conversationId.value)
    console.log('Do they match?', message.conversationId.value === activeConversationId.value)
    
    const existingIndex = messages.value.findIndex(m => m.id.value === message.id.value)
    if (existingIndex >= 0) {
      messages.value[existingIndex] = message
      console.log('Updated existing message at index:', existingIndex)
    } else {
      messages.value.push(message)
      console.log('Added new message, total messages now:', messages.value.length)
    }
    
    // Force reactivity trigger
    console.log('Current activeMessages length:', activeMessages.value.length)
    console.log('Active conversation messages:', activeMessages.value.map(m => m.content))
  }

  const updateTypingIndicator = (indicator: TypingIndicator) => {
    const conversationIndicators = typingIndicators.value.get(indicator.conversationId) || []
    
    if (indicator.isTyping) {
      // Add or update typing indicator
      const existingIndex = conversationIndicators.findIndex(i => i.userId === indicator.userId)
      if (existingIndex >= 0) {
        conversationIndicators[existingIndex] = indicator
      } else {
        conversationIndicators.push(indicator)
      }
    } else {
      // Remove typing indicator
      const filteredIndicators = conversationIndicators.filter(i => i.userId !== indicator.userId)
      typingIndicators.value.set(indicator.conversationId, filteredIndicators)
      return
    }
    
    typingIndicators.value.set(indicator.conversationId, conversationIndicators)
    
    // Auto-remove typing indicator after 5 seconds
    setTimeout(() => {
      const currentIndicators = typingIndicators.value.get(indicator.conversationId) || []
      const filteredIndicators = currentIndicators.filter(i => 
        i.userId !== indicator.userId || 
        new Date().getTime() - new Date(i.timestamp).getTime() < 5000
      )
      typingIndicators.value.set(indicator.conversationId, filteredIndicators)
    }, 5000)
  }

  const initializeSignalR = async () => {
    setupSignalRListeners()
    
    try {
      await signalRService.connect()
    } catch (err) {
      console.warn('SignalR connection failed, using REST API fallback:', err)
    }
  }

  const cleanupSignalR = () => {
    unsubscribeFunctions.forEach(fn => fn())
    unsubscribeFunctions = []
  }

  return {
    // State
    conversations,
    messages,
    typingIndicators,
    userPresences,
    activeConversationId,
    isLoading,
    error,
    
    // Computed
    activeConversation,
    activeMessages,
    activeTypingUsers,
    onlineUsers,
    
    // Actions
    fetchConversations,
    fetchMessages,
    sendMessage,
    startConversation,
    setActiveConversation,
    clearActiveConversation,
    startTyping,
    stopTyping,
    markMessageAsRead,
    initializeSignalR,
    cleanupSignalR
  }
})