<template>
  <div class="conversation-view">
    <!-- Connection Status Component -->
    <ConnectionStatus />
    
    <div class="header">
      <button @click="navigateToMessages" class="back-button">←</button>
      <div class="conversation-info">
        <h1>Conversation</h1>
        <!-- Online Users Count -->
        <div v-if="onlineUsers.length > 0" class="online-count">
          {{ onlineUsers.length }} online
        </div>
      </div>
      <!-- Call Controls -->
      <div class="header-actions">
        <CallButton
          v-if="conversationId && calleeId"
          :conversationId="conversationId"
          :calleeId="calleeId"
          :calleeUsername="calleeUsername"
        />
      </div>
    </div>
    
    <!-- Error message -->
    <div v-if="error" class="error-message">
      {{ error }}
      <button @click="error = ''" class="close-error">×</button>
    </div>
    
    <!-- Loading state -->
    <div v-if="isLoading" class="loading-state">
      Loading messages...
    </div>
    
    <!-- Empty state -->
    <div v-else-if="activeMessages.length === 0 && !error" class="empty-state">
      No messages found. Start the conversation!
    </div>

    <!-- Messages Container -->
    <div v-else class="messages-container" ref="messagesContainer">
      <div v-for="msg in activeMessages" :key="msg.id.value" class="message">
        <div class="message-header">
          <div class="user-info">
            <img 
              v-if="msg.pictureUrl" 
              :src="msg.pictureUrl" 
              :alt="msg.userName"
              class="user-avatar"
            />
            <div v-else class="user-avatar-placeholder">
              {{ msg.userName.charAt(0).toUpperCase() }}
            </div>
            <span class="username">{{ msg.userName }}</span>
            <UserPresenceIndicator 
              :userId="msg.senderId.value" 
              class="user-presence"
            />
          </div>
          <span class="timestamp">{{ formatTimestamp(msg.sentAt) }}</span>
        </div>
        <p class="content">{{ msg.content }}</p>
      </div>
      
      <!-- Typing Indicators -->
      <div v-if="activeTypingUsers.length > 0" class="typing-indicators">
        <div v-for="indicator in activeTypingUsers" :key="indicator.userId" class="typing-indicator">
          <div class="typing-dots">
            <span></span>
            <span></span>
            <span></span>
          </div>
          <span class="typing-text">{{ getUserName(indicator.userId) }} is typing...</span>
        </div>
      </div>
    </div>

    <!-- Message Input -->
    <div class="message-input">
      <textarea 
        v-model="newMessage" 
        placeholder="Type a message... (Press Enter to send, Shift+Enter for new line)"
        @keydown="handleKeydown"
        @input="handleTyping"
        :disabled="isSendingMessage"
        rows="1"
        ref="messageInput"
      ></textarea>
      <button 
        @click="sendMessage" 
        :disabled="!newMessage.trim() || isSendingMessage"
        class="send-button"
      >
        <span v-if="isSendingMessage" class="loading-spinner"></span>
        <span v-else>Send</span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useMessageStore } from '../stores/messageStore'
import { useAccountStore } from '../stores/accountStore'
import ConnectionStatus from '../components/ConnectionStatus.vue'
import UserPresenceIndicator from '../components/UserPresenceIndicator.vue'
import CallButton from '../components/CallButton.vue'
// Imports for IncomingCallModal and ActiveCallInterface might be needed if they are used
// import IncomingCallModal from '../components/IncomingCallModal.vue'
// import ActiveCallInterface from '../components/ActiveCallInterface.vue'

const route = useRoute()
const router = useRouter()
const messageStore = useMessageStore()
const accountStore = useAccountStore()

// Reactive data
const conversationId = ref(route.params.conversationId as string)
const newMessage = ref('')
const isSendingMessage = ref(false)
const messagesContainer = ref<HTMLElement | undefined>()
const messageInput = ref<HTMLTextAreaElement | undefined>()
const typingTimer = ref<number | null>(null)
const isTyping = ref(false)

// Computed properties
const activeMessages = computed(() => messageStore.activeMessages)
const activeTypingUsers = computed(() => messageStore.activeTypingUsers)
const onlineUsers = computed(() => messageStore.onlineUsers)
const isLoading = computed(() => messageStore.isLoading)
const error = computed({
  get: () => messageStore.error,
  set: (value: string | null) => { messageStore.error = value }
})

// Get the conversation and other participant for calling
const activeConversation = computed(() => messageStore.activeConversation)
const otherParticipant = computed(() => {
  const conversation = activeConversation.value
  const currentUserId = accountStore.accountId
  
  if (!conversation || !currentUserId) {
    return null
  }
  
  // Find the participant who is NOT the current user
  return conversation.participants.find(p => p.id.value !== currentUserId) || null
})

const calleeId = computed(() => otherParticipant.value?.id.value || '')
const calleeUsername = computed(() => otherParticipant.value?.userName || 'Unknown')

// Navigation
function navigateToMessages() {
  router.push({ name: 'Messages' })
}

// Message handling
// FIX: The sendMessage function is now decoupled from the event.
// It no longer accepts an event argument, resolving the type conflict.
async function sendMessage() {
  const trimmedMessage = newMessage.value.trim()
  if (!trimmedMessage || isSendingMessage.value) {
    return
  }

  isSendingMessage.value = true

  try {
    if (isTyping.value) {
      await messageStore.stopTyping(conversationId.value)
      isTyping.value = false
    }

    await messageStore.sendMessage(conversationId.value, trimmedMessage)
    
    newMessage.value = ''
    scrollToBottom()
  } catch (err) {
    console.error('Failed to send message:', err)
  } finally {
    isSendingMessage.value = false
  }
}

// Typing indicator handling
async function handleTyping() {
  if (!conversationId.value) return

  if (typingTimer.value) {
    clearTimeout(typingTimer.value)
  }

  if (!isTyping.value && newMessage.value.trim()) {
    isTyping.value = true
    await messageStore.startTyping(conversationId.value)
  }

  typingTimer.value = window.setTimeout(async () => {
    if (isTyping.value) {
      isTyping.value = false
      await messageStore.stopTyping(conversationId.value)
    }
  }, 3000)
}

// Keyboard handling
// FIX: Event-specific logic like preventDefault() is now handled here,
// before calling the generic sendMessage() function.
function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Enter' && !event.shiftKey) {
    event.preventDefault() // Prevent adding a new line to the textarea
    sendMessage()
  }
}

// Utility functions
function scrollToBottom() {
  nextTick(() => {
    if (messagesContainer.value) {
      messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
    }
  })
}

function formatTimestamp(timestamp: string): string {
  const date = new Date(timestamp)
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

function getUserName(userId: string): string {
  const participant = activeConversation.value?.participants.find(p => p.id.value === userId);
  return participant?.userName || `User ${userId.slice(0, 8)}`
}

// Lifecycle hooks
onMounted(async () => {
  try {
    await messageStore.initializeSignalR()
    await messageStore.setActiveConversation(conversationId.value)
    scrollToBottom()
  } catch (err) {
    console.error('Failed to initialize conversation:', err)
  }
})

onUnmounted(async () => {
  if (isTyping.value) {
    await messageStore.stopTyping(conversationId.value)
  }
  
  if (typingTimer.value) {
    clearTimeout(typingTimer.value)
  }
  
  await messageStore.clearActiveConversation()
  messageStore.cleanupSignalR()
})

// Watch for new messages and scroll to bottom
watch(activeMessages, () => {
  scrollToBottom()
}, { deep: true })

// Watch route changes
watch(() => route.params.conversationId, async (newId) => {
  if (newId && newId !== conversationId.value) {
    conversationId.value = newId as string
    await messageStore.setActiveConversation(conversationId.value)
  }
})
</script>

<style scoped>
.conversation-view {
  padding: 20px;
  display: flex;
  flex-direction: column;
  height: calc(100vh - 40px);
  position: relative;
}

.header {
  display: flex;
  align-items: center;
  margin-bottom: 20px;
  flex-shrink: 0;
}

.back-button {
  margin-right: 15px;
  padding: 0;
  width: 30px;
  height: 30px;
  background: none;
  border: none;
  font-size: 24px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.back-button:hover {
  color: #3498db;
}

.conversation-info {
  display: flex;
  align-items: center;
  gap: 15px;
  flex-grow: 1;
}

.conversation-info h1 {
  margin: 0;
  font-size: 20px;
}

.online-count {
  background: #28a745;
  color: white;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.loading-state,
.empty-state {
  display: flex;
  justify-content: center;
  align-items: center;
  flex: 1;
  color: #666;
}

.messages-container {
  flex: 1;
  overflow-y: auto;
  margin-bottom: 20px;
  padding-right: 10px;
}

.message {
  border: 1px solid #e0e0e0;
  border-radius: 12px;
  padding: 12px;
  margin-bottom: 12px;
  background-color: #f9f9f9;
  transition: background-color 0.2s;
}

.message:hover {
  background-color: #f5f5f5;
}

.message-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
}

.user-avatar {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  object-fit: cover;
}

.user-avatar-placeholder {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background-color: #3498db;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: bold;
}

.username {
  font-weight: 600;
  color: #333;
}

.user-presence {
  margin-left: 4px;
}

.timestamp {
  font-size: 0.8rem;
  color: #666;
}

.content {
  margin: 0;
  white-space: pre-wrap;
  line-height: 1.4;
  color: #333;
}

.typing-indicators {
  padding: 12px;
  margin-bottom: 12px;
}

.typing-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #666;
  font-style: italic;
  margin-bottom: 4px;
}

.typing-dots {
  display: flex;
  gap: 2px;
}

.typing-dots span {
  width: 4px;
  height: 4px;
  background-color: #666;
  border-radius: 50%;
  animation: typing-pulse 1.4s ease-in-out infinite;
}

.typing-dots span:nth-child(2) {
  animation-delay: 0.2s;
}

.typing-dots span:nth-child(3) {
  animation-delay: 0.4s;
}

@keyframes typing-pulse {
  0%, 60%, 100% {
    opacity: 0.3;
    transform: scale(1);
  }
  30% {
    opacity: 1;
    transform: scale(1.2);
  }
}

.typing-text {
  font-size: 0.9rem;
}

.message-input {
  display: flex;
  gap: 10px;
  align-items: flex-end;
  flex-shrink: 0;
}

.message-input textarea {
  flex: 1;
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 8px;
  resize: none; /* Changed from vertical to prevent manual resize conflicts */
  min-height: 40px;
  max-height: 120px;
  font-family: inherit;
  font-size: inherit;
  line-height: 1.4;
  transition: border-color 0.2s;
}

.message-input textarea:focus {
  outline: none;
  border-color: #3498db;
}

.message-input textarea:disabled {
  background-color: #f5f5f5;
  cursor: not-allowed;
}

.send-button {
  padding: 12px 18px;
  background-color: #3498db;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  min-width: 80px;
  height: 46px; /* Adjusted to match textarea padding */
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 500;
  transition: background-color 0.2s;
}

.send-button:hover:not(:disabled) {
  background-color: #2980b9;
}

.send-button:disabled {
  background-color: #bdc3c7;
  cursor: not-allowed;
}

.loading-spinner {
  width: 16px;
  height: 16px;
  border: 2px solid #ffffff;
  border-top: 2px solid transparent;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-message {
  background-color: #f8d7da;
  color: #721c24;
  padding: 12px;
  border-radius: 8px;
  margin-bottom: 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border: 1px solid #f5c6cb;
  flex-shrink: 0;
}

.close-error {
  background: none;
  border: none;
  font-size: 18px;
  cursor: pointer;
  color: #721c24;
  padding: 0;
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.close-error:hover {
  background-color: rgba(114, 28, 36, 0.1);
  border-radius: 50%;
}

/* Custom scrollbar */
.messages-container::-webkit-scrollbar {
  width: 6px;
}

.messages-container::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.messages-container::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.messages-container::-webkit-scrollbar-thumb:hover {
  background: #a1a1a1;
}
</style>