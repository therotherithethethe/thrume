<template>
  <div 
    v-if="shouldShow" 
    class="connection-status"
    :class="statusClass"
  >
    <div class="status-content">
      <div class="status-indicator">
        <div class="indicator-dot" :class="dotClass"></div>
        <span class="status-text">{{ statusText }}</span>
      </div>
      
      <div v-if="connectionState.reconnectAttempts > 0" class="reconnect-info">
        Attempt {{ connectionState.reconnectAttempts }}
      </div>
      
      <button 
        v-if="connectionState.status === 'disconnected'" 
        @click="reconnect"
        class="reconnect-button"
        :disabled="isReconnecting"
      >
        {{ isReconnecting ? 'Connecting...' : 'Reconnect' }}
      </button>
    </div>
    
    <button 
      @click="dismiss" 
      class="dismiss-button"
      aria-label="Dismiss notification"
    >
      ×
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import signalRService from '../services/signalRService'
import type { ConnectionState } from '../types/signalr'

const isDismissed = ref(false)
const isReconnecting = ref(false)

const connectionState = computed<ConnectionState>(() => signalRService.state)

const shouldShow = computed(() => {
  if (isDismissed.value) return false
  return connectionState.value.status !== 'connected'
})

const statusClass = computed(() => {
  return `status-${connectionState.value.status}`
})

const dotClass = computed(() => {
  switch (connectionState.value.status) {
    case 'connected':
      return 'dot-connected'
    case 'connecting':
    case 'reconnecting':
      return 'dot-connecting'
    case 'disconnected':
      return 'dot-disconnected'
    default:
      return 'dot-disconnected'
  }
})

const statusText = computed(() => {
  switch (connectionState.value.status) {
    case 'connected':
      return 'Connected'
    case 'connecting':
      return 'Connecting...'
    case 'reconnecting':
      return 'Reconnecting...'
    case 'disconnected':
      return connectionState.value.error ? 'Connection failed' : 'Disconnected'
    default:
      return 'Unknown status'
  }
})

const reconnect = async () => {
  isReconnecting.value = true
  try {
    await signalRService.connect()
    isDismissed.value = true
  } catch (error) {
    console.error('Manual reconnection failed:', error)
  } finally {
    isReconnecting.value = false
  }
}

const dismiss = () => {
  isDismissed.value = true
}

// Reset dismissed state when connection is restored
watch(() => connectionState.value.status, (newStatus) => {
  if (newStatus === 'connected') {
    isDismissed.value = false
  }
})
</script>

<style scoped>
.connection-status {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 1000;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  font-size: 14px;
  font-weight: 500;
  max-width: 300px;
  animation: slideIn 0.3s ease-out;
}

@keyframes slideIn {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

.status-connected {
  background-color: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.status-connecting,
.status-reconnecting {
  background-color: #fff3cd;
  color: #856404;
  border: 1px solid #ffeaa7;
}

.status-disconnected {
  background-color: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.status-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
}

.indicator-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}

.dot-connected {
  background-color: #28a745;
}

.dot-connecting {
  background-color: #ffc107;
  animation: pulse 1.5s ease-in-out infinite;
}

.dot-disconnected {
  background-color: #dc3545;
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}

.status-text {
  font-weight: 600;
}

.reconnect-info {
  font-size: 12px;
  opacity: 0.8;
}

.reconnect-button {
  padding: 4px 8px;
  border: none;
  border-radius: 4px;
  background-color: rgba(0, 0, 0, 0.1);
  color: inherit;
  font-size: 12px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.reconnect-button:hover:not(:disabled) {
  background-color: rgba(0, 0, 0, 0.2);
}

.reconnect-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.dismiss-button {
  background: none;
  border: none;
  color: inherit;
  font-size: 18px;
  cursor: pointer;
  padding: 0;
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: background-color 0.2s;
}

.dismiss-button:hover {
  background-color: rgba(0, 0, 0, 0.1);
}
</style>