<template>
  <div v-if="typingUsers.length > 0" class="typing-indicator">
    <div class="typing-content">
      <div class="typing-dots">
        <span></span>
        <span></span>
        <span></span>
      </div>
      <span class="typing-text">
        {{ typingText }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { TypingIndicator as TypingIndicatorType } from '../types/signalr'

interface Props {
  typingUsers: TypingIndicatorType[]
}

const props = defineProps<Props>()

const typingText = computed(() => {
  const users = props.typingUsers
  if (users.length === 0) return ''
  
  if (users.length === 1) {
    return `${getUserName(users[0].userId)} is typing...`
  } else if (users.length === 2) {
    return `${getUserName(users[0].userId)} and ${getUserName(users[1].userId)} are typing...`
  } else {
    return `${users.length} people are typing...`
  }
})

function getUserName(userId: string): string {
  // This would be improved with user lookup functionality
  return `User ${userId.slice(0, 8)}`
}
</script>

<style scoped>
.typing-indicator {
  padding: 8px 12px;
  margin: 4px 0;
  background-color: #f8f9fa;
  border-radius: 12px;
  border: 1px solid #e9ecef;
}

.typing-content {
  display: flex;
  align-items: center;
  gap: 8px;
}

.typing-dots {
  display: flex;
  gap: 2px;
}

.typing-dots span {
  width: 4px;
  height: 4px;
  background-color: #6c757d;
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
  color: #6c757d;
  font-style: italic;
}
</style>