<template>
  <div v-if="callStore.hasIncomingCall" class="incoming-call-overlay">
    <div class="incoming-call-modal">
      <div class="caller-info">
        <div class="avatar-container">
          <div class="avatar">
            {{ callerInitials }}
          </div>
          <div class="call-type-indicator">
            <svg v-if="callType === CallType.Video" width="20" height="20" viewBox="0 0 24 24" fill="white">
              <path d="M17 10.5V7c0-.55-.45-1-1-1H4c-.55 0-1 .45-1 1v10c0 .55.45 1 1 1h12c.55 0 1-.45 1-1v-3.5l4 4v-11l-4 4z"/>
            </svg>
            <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="white">
              <path d="M20.01 15.38c-1.23 0-2.42-.2-3.53-.56-.35-.12-.74-.03-1.01.24l-1.57 1.97c-2.83-1.35-5.48-3.9-6.89-6.83l1.95-1.66c.27-.28.35-.67.24-1.02-.37-1.11-.56-2.3-.56-3.53 0-.54-.45-.99-.99-.99H4.19C3.65 3 3 3.24 3 3.99 3 13.28 10.73 21 20.01 21c.71 0 .99-.63.99-1.18v-3.45c0-.54-.45-.99-.99-.99z"/>
            </svg>
          </div>
        </div>
        
        <div class="caller-details">
          <h3 class="caller-name">{{ callerName }}</h3>
          <p class="call-type-text">{{ callTypeText }} call</p>
          <p class="call-status">Incoming call...</p>
        </div>
      </div>
      
      <div class="call-actions">
        <button @click="rejectCall" class="action-btn reject-btn" title="Reject Call">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
            <path d="M12 9c-1.6 0-3.15.25-4.6.72v3.1c0 .39-.23.74-.56.9-.98.49-1.87 1.12-2.66 1.85-.18.18-.43.28-.69.28-.26 0-.51-.1-.71-.29L.29 13.08c-.18-.17-.29-.42-.29-.7 0-.28.11-.53.29-.71C3.34 8.78 7.46 7 12 7s8.66 1.78 11.71 4.67c.18.18.29.43.29.71 0 .28-.11.53-.29.7l-2.48 2.48c-.18.18-.43.29-.69.29-.26 0-.51-.1-.69-.28-.79-.73-1.68-1.36-2.66-1.85-.33-.16-.56-.5-.56-.9v-3.1C15.15 9.25 13.6 9 12 9z"/>
          </svg>
        </button>
        
        <button @click="acceptCall" class="action-btn accept-btn" title="Accept Call">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
            <path d="M20.01 15.38c-1.23 0-2.42-.2-3.53-.56-.35-.12-.74-.03-1.01.24l-1.57 1.97c-2.83-1.35-5.48-3.9-6.89-6.83l1.95-1.66c.27-.28.35-.67.24-1.02-.37-1.11-.56-2.3-.56-3.53 0-.54-.45-.99-.99-.99H4.19C3.65 3 3 3.24 3 3.99 3 13.28 10.73 21 20.01 21c.71 0 .99-.63.99-1.18v-3.45c0-.54-.45-.99-.99-.99z"/>
          </svg>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useCallStore } from '../stores/callStore'
import { CallType } from '../types/signalr'

const callStore = useCallStore()

const incomingCall = computed(() => callStore.incomingCall)

const callerName = computed(() => {
  return incomingCall.value?.callerUsername || 'Unknown'
})

const callerInitials = computed(() => {
  const name = callerName.value
  return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
})

const callType = computed(() => {
  return incomingCall.value?.callType || CallType.Voice
})

const callTypeText = computed(() => {
  return callType.value === CallType.Video ? 'Video' : 'Voice'
})

const acceptCall = async () => {
  try {
    await callStore.acceptCall()
  } catch (error) {
    console.error('Failed to accept call:', error)
  }
}

const rejectCall = async () => {
  try {
    await callStore.rejectCall()
  } catch (error) {
    console.error('Failed to reject call:', error)
  }
}
</script>

<style scoped>
.incoming-call-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  backdrop-filter: blur(4px);
}

.incoming-call-modal {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 20px;
  padding: 32px;
  min-width: 300px;
  max-width: 400px;
  text-align: center;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  animation: slideIn 0.3s ease-out;
}

@keyframes slideIn {
  from {
    transform: translateY(-30px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}

.caller-info {
  margin-bottom: 32px;
}

.avatar-container {
  position: relative;
  display: inline-block;
  margin-bottom: 16px;
}

.avatar {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  font-weight: bold;
  color: white;
  border: 4px solid rgba(255, 255, 255, 0.3);
}

.call-type-indicator {
  position: absolute;
  bottom: -4px;
  right: -4px;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: #10b981;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 3px solid white;
}

.caller-details {
  color: white;
}

.caller-name {
  font-size: 24px;
  font-weight: bold;
  margin: 0 0 8px 0;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
}

.call-type-text {
  font-size: 16px;
  margin: 0 0 4px 0;
  opacity: 0.9;
}

.call-status {
  font-size: 14px;
  margin: 0;
  opacity: 0.8;
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0%, 100% {
    opacity: 0.8;
  }
  50% {
    opacity: 1;
  }
}

.call-actions {
  display: flex;
  justify-content: center;
  gap: 32px;
}

.action-btn {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  color: white;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.action-btn:hover {
  transform: scale(1.1);
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.3);
}

.action-btn:active {
  transform: scale(0.95);
}

.reject-btn {
  background: linear-gradient(135deg, #ef4444, #dc2626);
}

.reject-btn:hover {
  background: linear-gradient(135deg, #dc2626, #b91c1c);
}

.accept-btn {
  background: linear-gradient(135deg, #10b981, #059669);
}

.accept-btn:hover {
  background: linear-gradient(135deg, #059669, #047857);
}
</style>