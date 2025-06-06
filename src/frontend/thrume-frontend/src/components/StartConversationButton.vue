<template>
  <div v-if="shouldShowButton" class="start-conversation-container">
    <!-- Default state: "Start Conversation" button -->
    <button
      v-if="!isLoading && !error && !successState"
      @click="handleStartConversation"
      class="start-conversation-btn"
      :aria-label="`Start conversation with ${targetUserName}`"
      :disabled="!canStartConversation"
    >
      Start Conversation
    </button>

    <!-- Loading state: Spinner with "Starting..." text -->
    <div v-if="isLoading" class="start-conversation-loading">
      <div class="spinner"></div>
      <span class="loading-text">Starting...</span>
    </div>

    <!-- Success state: Brief "Conversation started!" before navigation -->
    <div v-if="successState" class="start-conversation-success">
      <span class="success-text">Conversation started!</span>
    </div>

    <!-- Error state: Error message with retry option -->
    <div v-if="error" class="start-conversation-error">
      <p class="error-message">{{ error }}</p>
      <button
        @click="handleRetry"
        class="retry-btn"
        :aria-label="'Retry starting conversation with ' + targetUserName"
      >
        Try Again
      </button>
    </div>
  </div>

  <!-- Show appropriate messages when conditions aren't met -->
  <div v-else-if="!isCurrentUser && !bothUsersFollowEachOther" class="follow-requirement-message">
    <p class="requirement-text">
      Can't start conservation
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAccountStore } from '../stores/accountStore';
import { startConversation } from '../services/messageService';
import type { Conversation } from '../types';

// Props
const props = defineProps<{
  targetUserName: string;
  isFollowingUser: boolean;
  isUserFollowingMe: boolean;
}>();

// Composables
const router = useRouter();
const accountStore = useAccountStore();

// State
const isLoading = ref(false);
const error = ref<string | null>(null);
const successState = ref(false);

// Computed properties
const currentUser = computed(() => accountStore.currentAccount);
const currentUsername = computed(() => accountStore.accountUsername);

const isCurrentUser = computed(() => {
  return currentUsername.value === props.targetUserName;
});

const bothUsersFollowEachOther = computed(() => {
  return props.isFollowingUser && props.isUserFollowingMe;
});

const canStartConversation = computed(() => {
  return bothUsersFollowEachOther.value && !isCurrentUser.value;
});

const shouldShowButton = computed(() => {
  // Only show button if:
  // 1. Target user is not the current user
  // 2. Both users follow each other (mutual following required)
  return !isCurrentUser.value && bothUsersFollowEachOther.value;
});

// Methods
const handleStartConversation = async () => {
  if (!canStartConversation.value) {
    return;
  }

  isLoading.value = true;
  error.value = null;

  try {
    console.log(`Starting conversation with ${props.targetUserName}`);
    
    const conversation: Conversation = await startConversation(props.targetUserName);
    
    console.log('Conversation created successfully:', conversation);
    
    // Show success state briefly
    successState.value = true;
    
    // Navigate to the new conversation after a short delay
    setTimeout(() => {
      const conversationId = conversation.id.value;
      router.push(`/messages/${conversationId}`);
    }, 1000);

  } catch (err: any) {
    console.error('Error starting conversation:', err);
    
    // Handle different types of errors
    if (err.response) {
      if (err.response.status === 400) {
        // Handle 400 responses (existing conversation or not following each other)
        const errorMessage = err.response.data?.message;
        if (errorMessage && errorMessage.includes('already exists')) {
          error.value = 'A conversation with this user already exists.';
        } else if (errorMessage && errorMessage.includes('follow')) {
          error.value = 'Both users must follow each other to start a conversation.';
        } else {
          error.value = errorMessage || 'Unable to start conversation.';
        }
      } else if (err.response.status === 404) {
        error.value = 'User not found. Please check the username.';
      } else if (err.response.status >= 500) {
        error.value = 'Server error. Please try again later.';
      } else {
        error.value = 'An unexpected error occurred. Please try again.';
      }
    } else if (err.code === 'NETWORK_ERROR' || !navigator.onLine) {
      // Handle network errors
      error.value = 'Network error. Please check your connection and try again.';
    } else if (err.message) {
      error.value = err.message;
    } else {
      error.value = 'Failed to start conversation. Please try again.';
    }
  } finally {
    isLoading.value = false;
  }
};

const handleRetry = () => {
  error.value = null;
  successState.value = false;
  handleStartConversation();
};

// Check authentication on mount
onMounted(() => {
  if (!accountStore.isLoggedIn) {
    console.warn('User is not logged in - StartConversationButton should not be shown');
  }
});
</script>

<style scoped>
.start-conversation-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.start-conversation-btn {
  padding: 12px 24px;
  border: none;
  border-radius: 6px;
  background-color: #3897f0;
  color: white;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s ease;
  min-width: 140px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.start-conversation-btn:hover:not(:disabled) {
  background-color: #2980d9;
  transform: translateY(-1px);
  box-shadow: 0 2px 8px rgba(56, 151, 240, 0.3);
}

.start-conversation-btn:active:not(:disabled) {
  transform: translateY(0);
  box-shadow: 0 1px 4px rgba(56, 151, 240, 0.3);
}

.start-conversation-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  background-color: #ccc;
}

.start-conversation-loading {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  color: #666;
  min-width: 140px;
  justify-content: center;
}

.spinner {
  width: 16px;
  height: 16px;
  border: 2px solid #f3f3f3;
  border-top: 2px solid #3897f0;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.loading-text {
  font-size: 14px;
  font-weight: 500;
}

.start-conversation-success {
  padding: 12px 24px;
  min-width: 140px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.success-text {
  color: #27ae60;
  font-weight: 600;
  font-size: 14px;
}

.start-conversation-error {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 12px;
  border-radius: 6px;
  background-color: #fff5f5;
  border: 1px solid #fed7d7;
  max-width: 280px;
}

.error-message {
  margin: 0;
  color: #e53e3e;
  font-size: 14px;
  text-align: center;
  line-height: 1.4;
}

.retry-btn {
  padding: 8px 16px;
  border: 1px solid #e53e3e;
  border-radius: 4px;
  background-color: white;
  color: #e53e3e;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.retry-btn:hover {
  background-color: #e53e3e;
  color: white;
}

.follow-requirement-message {
  padding: 12px;
  border-radius: 6px;
  background-color: #fff8e1;
  border: 1px solid #ffcc02;
  max-width: 280px;
}

.requirement-text {
  margin: 0;
  color: #f57c00;
  font-size: 14px;
  text-align: center;
  line-height: 1.4;
}

/* Responsive design for mobile */
@media (max-width: 768px) {
  .start-conversation-btn {
    padding: 10px 20px;
    font-size: 13px;
    min-width: 120px;
  }
  
  .start-conversation-error,
  .follow-requirement-message {
    max-width: 100%;
    margin: 0 8px;
  }
  
  .error-message,
  .requirement-text {
    font-size: 13px;
  }
}

/* High contrast mode support */
@media (prefers-contrast: high) {
  .start-conversation-btn {
    border: 2px solid #000;
  }
  
  .start-conversation-error {
    border: 2px solid #e53e3e;
  }
  
  .follow-requirement-message {
    border: 2px solid #f57c00;
  }
}

/* Reduced motion support */
@media (prefers-reduced-motion: reduce) {
  .start-conversation-btn {
    transition: none;
  }
  
  .spinner {
    animation: none;
  }
  
  .start-conversation-btn:hover:not(:disabled) {
    transform: none;
  }
}

/* Focus styles for keyboard navigation */
.start-conversation-btn:focus,
.retry-btn:focus {
  outline: 2px solid #3897f0;
  outline-offset: 2px;
}

/* Dark mode support (if implemented in the app) */
@media (prefers-color-scheme: dark) {
  .start-conversation-error {
    background-color: #2d1b1b;
    border-color: #e53e3e;
  }
  
  .follow-requirement-message {
    background-color: #2d2416;
    border-color: #ffcc02;
  }
  
  .retry-btn {
    background-color: #2d1b1b;
  }
}
</style>