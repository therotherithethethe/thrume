<script setup lang="ts">
import { ref, onMounted, nextTick } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { getMessages, sendMessage as sendMessageAPI, SendMessageResponse } from '../services/messageService';
import { Message } from '../types';
import { useAccountStore } from '../stores/accountStore';

const route = useRoute();
const router = useRouter();
const accountStore = useAccountStore();

function navigateToMessages() {
  router.push({ name: 'Messages' });
}

const conversationId = ref(route.params.conversationId as string);
const messages = ref<Message[]>([]);
const newMessage = ref('');
const isSendingMessage = ref(false);
const errorMessage = ref('');
const messagesContainer = ref<HTMLElement>();

onMounted(async () => {
  try {
    messages.value = await getMessages(conversationId.value);
    scrollToBottom();
  } catch (error) {
    console.error('Failed to fetch messages:', error);
    errorMessage.value = 'Failed to load messages. Please try again.';
  }
});

function scrollToBottom() {
  nextTick(() => {
    if (messagesContainer.value) {
      messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight;
    }
  });
}

async function sendMessage(event?: KeyboardEvent) {
  // Handle Shift+Enter for new line
  if (event && event.shiftKey) {
    return;
  }
  
  // Prevent default if it's an Enter key event
  if (event) {
    event.preventDefault();
  }

  // Validate message
  const trimmedMessage = newMessage.value.trim();
  if (!trimmedMessage || isSendingMessage.value) {
    return;
  }

  // Clear any previous errors
  errorMessage.value = '';
  isSendingMessage.value = true;

  try {
    // Create optimistic message for immediate UI update
    const optimisticMessage: Message = {
      id: { value: `temp-${Date.now()}` },
      conversationId: { value: conversationId.value },
      content: trimmedMessage,
      sentAt: new Date().toISOString().replace('Z', '+00:00'),
      userName: accountStore.currentAccount?.username || 'You',
      senderId: { value: accountStore.currentAccount?.id || 'current-user' },
      pictureUrl: accountStore.currentAccount?.profilePictureUrl || null
    };

    // Add message to UI immediately (optimistic update)
    messages.value.push(optimisticMessage);
    scrollToBottom();

    // Clear input field
    const messageToSend = trimmedMessage;
    newMessage.value = '';

    // Send message to API
    const response: SendMessageResponse = await sendMessageAPI(conversationId.value, messageToSend);

    // Transform response to Message format
    const newMessageFromAPI: Message = {
      id: response.id,
      conversationId: response.conversationId,
      senderId: response.senderId,
      content: response.content,
      sentAt: response.timestamp,
      userName: accountStore.currentAccount?.username || 'You',
      pictureUrl: accountStore.currentAccount?.profilePictureUrl || null
    };

    // Replace optimistic message with actual response
    const lastIndex = messages.value.length - 1;
    if (messages.value[lastIndex].id.value === optimisticMessage.id.value) {
      messages.value[lastIndex] = newMessageFromAPI;
    } else {
      // If the optimistic message is not at the end, find and replace it
      const optimisticIndex = messages.value.findIndex(msg => msg.id.value === optimisticMessage.id.value);
      if (optimisticIndex !== -1) {
        messages.value[optimisticIndex] = newMessageFromAPI;
      } else {
        // If not found, just add the new message
        messages.value.push(newMessageFromAPI);
      }
    }

    scrollToBottom();
  } catch (error) {
    console.error('Failed to send message:', error);
    
    // Remove the optimistic message on failure
    const optimisticIndex = messages.value.findIndex(msg => msg.id.value.startsWith('temp-'));
    if (optimisticIndex !== -1) {
      messages.value.splice(optimisticIndex, 1);
    }

    // Restore the message to input field
    newMessage.value = trimmedMessage;
    
    // Show error message
    errorMessage.value = 'Failed to send message. Please try again.';
  } finally {
    isSendingMessage.value = false;
  }
}

function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Enter' && !event.shiftKey) {
    sendMessage(event);
  }
}
</script>

<template>
  <div class="conversation-view">
    <div class="header">
      <button @click="navigateToMessages" class="back-button">←</button>
      <h1>Conversation</h1>
    </div>
    
    <!-- Error message -->
    <div v-if="errorMessage" class="error-message">
      {{ errorMessage }}
      <button @click="errorMessage = ''" class="close-error">×</button>
    </div>
    
    <div v-if="messages.length === 0 && !errorMessage" class="empty-state">
      No messages found
    </div>

    <div v-else class="messages-container" ref="messagesContainer">
      <div v-for="msg in messages" :key="msg.id.value" class="message">
        <div class="message-header">
          <span class="username">{{ msg.userName }}</span>
          <span class="timestamp">{{ new Date(msg.sentAt).toLocaleTimeString() }}</span>
        </div>
        <p class="content">{{ msg.content }}</p>
      </div>
    </div>

    <div class="message-input">
      <textarea 
        v-model="newMessage" 
        placeholder="Type a message... (Press Enter to send, Shift+Enter for new line)"
        @keydown="handleKeydown"
        :disabled="isSendingMessage"
        rows="1"
      ></textarea>
      <button 
        @click="() => sendMessage()" 
        :disabled="!newMessage.trim() || isSendingMessage"
        class="send-button"
      >
        <span v-if="isSendingMessage" class="loading-spinner"></span>
        <span v-else>Send</span>
      </button>
    </div>
  </div>
</template>

<style scoped>
.conversation-view {
  padding: 20px;
  display: flex;
  flex-direction: column;
  height: calc(100vh - 40px);
}

.header {
  display: flex;
  align-items: center;
  margin-bottom: 20px;
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

.messages-container {
  flex: 1;
  overflow-y: auto;
  margin-bottom: 20px;
}

.message {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 10px;
  margin-bottom: 10px;
  background-color: #f9f9f9;
}

.message-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 5px;
}

.username {
  font-weight: bold;
}

.timestamp {
  font-size: 0.8rem;
  color: #666;
}

.content {
  margin: 0;
  white-space: pre-wrap;
}

.message-input {
  display: flex;
  gap: 10px;
  align-items: flex-end;
}

.message-input textarea {
  flex: 1;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
  resize: vertical;
  min-height: 40px;
  max-height: 120px;
  font-family: inherit;
  font-size: inherit;
  line-height: 1.4;
}

.message-input textarea:disabled {
  background-color: #f5f5f5;
  cursor: not-allowed;
}

.send-button {
  padding: 10px 15px;
  background-color: #3498db;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  min-width: 70px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
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
  padding: 10px;
  border-radius: 4px;
  margin-bottom: 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border: 1px solid #f5c6cb;
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

.empty-state {
  text-align: center;
  padding: 40px;
  color: #666;
}
</style>