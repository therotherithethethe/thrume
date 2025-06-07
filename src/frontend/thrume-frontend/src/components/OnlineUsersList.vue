<template>
  <div class="online-users-list">
    <h3 class="users-title">
      <span class="online-indicator"></span>
      Online ({{ onlineUsers.length }})
    </h3>
    
    <div v-if="onlineUsers.length === 0" class="no-users">
      No users online
    </div>
    
    <div v-else class="users-container">
      <div 
        v-for="user in onlineUsers" 
        :key="user.userId"
        class="user-item"
        @click="$emit('userClick', user.userId)"
      >
        <UserPresenceIndicator :userId="user.userId" />
        <div class="user-info">
          <span class="user-name">{{ getUserName(user.userId) }}</span>
          <span class="user-status">Online</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'


import UserPresenceIndicator from './UserPresenceIndicator.vue'
import { useAccountStore } from '../stores/accountStore';
import { useMessageStore } from '../stores/messageStore';

interface Emits {
  userClick: [userId: string]
}

defineEmits<Emits>()

const messageStore = useMessageStore()
const accountStore = useAccountStore()

const onlineUsers = computed(() => messageStore.onlineUsers)

const getUserName = (userId: string) => {
  // Try to get user name from account store if available
  // This would need to be implemented based on your account data structure
  return `User ${userId.slice(0, 8)}...` // Fallback display
}
</script>

<style scoped>
.online-users-list {
  background: white;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.users-title {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 0 0 16px 0;
  font-size: 16px;
  font-weight: 600;
  color: #333;
}

.online-indicator {
  width: 8px;
  height: 8px;
  background-color: #28a745;
  border-radius: 50%;
  animation: pulse 2s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}

.no-users {
  color: #6c757d;
  font-style: italic;
  text-align: center;
  padding: 20px;
}

.users-container {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.user-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 12px;
  border-radius: 6px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.user-item:hover {
  background-color: #f8f9fa;
}

.user-info {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-width: 0;
}

.user-name {
  font-weight: 500;
  color: #333;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.user-status {
  font-size: 12px;
  color: #28a745;
  font-weight: 500;
}
</style>