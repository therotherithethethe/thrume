<template>
  <div class="call-controls">
    <button
      @click="initiateVoiceCall"
      :disabled="!canMakeCall"
      class="call-btn voice-call-btn"
      title="Voice Call"
    >
      <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
        <path d="M20.01 15.38c-1.23 0-2.42-.2-3.53-.56-.35-.12-.74-.03-1.01.24l-1.57 1.97c-2.83-1.35-5.48-3.9-6.89-6.83l1.95-1.66c.27-.28.35-.67.24-1.02-.37-1.11-.56-2.3-.56-3.53 0-.54-.45-.99-.99-.99H4.19C3.65 3 3 3.24 3 3.99 3 13.28 10.73 21 20.01 21c.71 0 .99-.63.99-1.18v-3.45c0-.54-.45-.99-.99-.99z"/>
      </svg>
    </button>
    
    <button
      @click="initiateVideoCall"
      :disabled="!canMakeCall"
      class="call-btn video-call-btn"
      title="Video Call"
    >
      <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
        <path d="M17 10.5V7c0-.55-.45-1-1-1H4c-.55 0-1 .45-1 1v10c0 .55.45 1 1 1h12c.55 0 1-.45 1-1v-3.5l4 4v-11l-4 4z"/>
      </svg>
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useCallStore } from '../stores/callStore'
import { CallType } from '../types/signalr'

interface Props {
  conversationId: string
  calleeId: string
  calleeUsername?: string
}

const props = defineProps<Props>()
const callStore = useCallStore()

const canMakeCall = computed(() => {
  return !callStore.isCallActive && !callStore.hasIncomingCall && !callStore.isInitiating
})

const initiateVoiceCall = async () => {
  console.log('🎯 [CallButton] Voice call button clicked!', {
    canMakeCall: canMakeCall.value,
    calleeId: props.calleeId,
    conversationId: props.conversationId,
    isCallActive: callStore.isCallActive,
    hasIncomingCall: callStore.hasIncomingCall,
    isInitiating: callStore.isInitiating
  })
  
  if (!canMakeCall.value) {
    console.log('❌ [CallButton] Cannot make call - conditions not met')
    return
  }
  
  try {
    console.log('📞 [CallButton] Starting voice call initiation...')
    await callStore.initiateCall(props.calleeId, CallType.Voice, props.conversationId)
    console.log('✅ [CallButton] Voice call initiation completed')
  } catch (error) {
    console.error('❌ [CallButton] Failed to initiate voice call:', error)
  }
}

const initiateVideoCall = async () => {
  console.log('🎯 [CallButton] Video call button clicked!', {
    canMakeCall: canMakeCall.value,
    calleeId: props.calleeId,
    conversationId: props.conversationId,
    isCallActive: callStore.isCallActive,
    hasIncomingCall: callStore.hasIncomingCall,
    isInitiating: callStore.isInitiating
  })
  
  if (!canMakeCall.value) {
    console.log('❌ [CallButton] Cannot make call - conditions not met')
    return
  }
  
  try {
    console.log('📞 [CallButton] Starting video call initiation...')
    await callStore.initiateCall(props.calleeId, CallType.Video, props.conversationId)
    console.log('✅ [CallButton] Video call initiation completed')
  } catch (error) {
    console.error('❌ [CallButton] Failed to initiate video call:', error)
  }
}
</script>

<style scoped>
.call-controls {
  display: flex;
  gap: 8px;
  align-items: center;
}

.call-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: none;
  border-radius: 50%;
  cursor: pointer;
  transition: all 0.2s ease;
  color: white;
}

.call-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.voice-call-btn {
  background-color: #10b981;
}

.voice-call-btn:hover:not(:disabled) {
  background-color: #059669;
  transform: scale(1.05);
}

.video-call-btn {
  background-color: #3b82f6;
}

.video-call-btn:hover:not(:disabled) {
  background-color: #2563eb;
  transform: scale(1.05);
}

.call-btn:active:not(:disabled) {
  transform: scale(0.95);
}
</style>