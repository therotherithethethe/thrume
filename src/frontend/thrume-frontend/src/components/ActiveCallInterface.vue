<template>
  <div v-if="callStore.isCallActive" class="active-call-interface">
    <div class="call-header">
      <div class="participant-info">
        <h3>{{ participantName }}</h3>
        <p class="call-status">{{ callStatusText }}</p>
        <p class="call-duration" v-if="callDuration">{{ formatDuration(callDuration) }}</p>
      </div>
    </div>

    <div class="video-container" v-if="isVideoCall">
      <video 
        ref="remoteVideo" 
        class="remote-video"
        autoplay 
        playsinline
        :muted="false"
      ></video>
      
      <video 
        ref="localVideo" 
        class="local-video"
        autoplay 
        muted 
        playsinline
      ></video>
      
      <div v-if="!hasRemoteStream" class="video-placeholder remote-placeholder">
        <div class="participant-avatar">
          {{ participantInitials }}
        </div>
        <p>{{ participantName }}</p>
      </div>
      
      <div v-if="!hasLocalStream" class="video-placeholder local-placeholder">
        <div class="participant-avatar">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
            <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
          </svg>
        </div>
        <p>You</p>
      </div>
    </div>

    <div class="audio-call-ui" v-else>
      <div class="participant-avatar-large">
        {{ participantInitials }}
      </div>
      <h2>{{ participantName }}</h2>
      <p class="call-status-large">{{ callStatusText }}</p>
    </div>

    <CallControls />

    <div v-if="callStore.callError" class="call-error">
      <p>{{ callStore.callError }}</p>
      <button @click="callStore.clearError" class="clear-error-btn">Dismiss</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch, onMounted, onUnmounted } from 'vue'
import { useCallStore } from '../stores/callStore'
import { useAccountStore } from '../stores/accountStore'
import { CallType, CallState } from '../types/signalr'
import CallControls from './CallControls.vue'

const callStore = useCallStore()
const accountStore = useAccountStore()

// Template refs
const remoteVideo = ref<HTMLVideoElement | null>(null)
const localVideo = ref<HTMLVideoElement | null>(null)

// Duration tracking
const callDuration = ref<number>(0)
let durationInterval: number | null = null

// Computed properties
const activeCall = computed(() => callStore.activeCall)

const participantName = computed(() => {
  if (!activeCall.value) return ''
  // If we're the caller, show callee name; if we're the callee, show caller name
  const accountStore = useAccountStore()
  const isWeCaller = activeCall.value.callerId === accountStore.accountId
  return isWeCaller ? 'Contact' : activeCall.value.callerUsername
})

const participantInitials = computed(() => {
  const name = participantName.value
  return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
})

const isVideoCall = computed(() => {
  return activeCall.value?.callType === CallType.Video
})

const callStatusText = computed(() => {
  if (!activeCall.value) return ''
  
  switch (activeCall.value.state) {
    case CallState.Initiating:
      return 'Initiating call...'
    case CallState.Ringing:
      return 'Ringing...'
    case CallState.Connecting:
      return 'Connecting...'
    case CallState.Connected:
      return 'Connected'
    case CallState.Reconnecting:
      return 'Reconnecting...'
    default:
      return 'Call in progress'
  }
})

const hasRemoteStream = computed(() => !!callStore.remoteStream)
const hasLocalStream = computed(() => !!callStore.localStream)

// Watch for stream changes and update video elements
watch(() => callStore.localStream, (newStream) => {
  if (localVideo.value && newStream) {
    localVideo.value.srcObject = newStream
  }
}, { immediate: true })

watch(() => callStore.remoteStream, (newStream) => {
  if (remoteVideo.value && newStream) {
    remoteVideo.value.srcObject = newStream
  }
}, { immediate: true })

// Watch for call state changes to manage duration
watch(() => activeCall.value?.state, (newState) => {
  if (newState === CallState.Connected && !durationInterval) {
    startDurationTimer()
  } else if (newState !== CallState.Connected && durationInterval) {
    stopDurationTimer()
  }
})

// Duration management
const startDurationTimer = () => {
  if (!activeCall.value) return
  
  const startTime = activeCall.value.startedAt.getTime()
  
  durationInterval = setInterval(() => {
    callDuration.value = Math.floor((Date.now() - startTime) / 1000)
  }, 1000)
}

const stopDurationTimer = () => {
  if (durationInterval) {
    clearInterval(durationInterval)
    durationInterval = null
  }
  callDuration.value = 0
}

const formatDuration = (seconds: number): string => {
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  const secs = seconds % 60

  if (hours > 0) {
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
  }
  return `${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
}

// Lifecycle
onMounted(() => {
  // Set up video streams if call is already active
  if (callStore.localStream && localVideo.value) {
    localVideo.value.srcObject = callStore.localStream
  }
  if (callStore.remoteStream && remoteVideo.value) {
    remoteVideo.value.srcObject = callStore.remoteStream
  }
  
  // Start duration timer if call is connected
  if (activeCall.value?.state === CallState.Connected) {
    startDurationTimer()
  }
})

onUnmounted(() => {
  stopDurationTimer()
})
</script>

<style scoped>
.active-call-interface {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: #000;
  display: flex;
  flex-direction: column;
  z-index: 999;
}

.call-header {
  position: absolute;
  top: 20px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 1001;
  text-align: center;
  color: white;
  background: rgba(0, 0, 0, 0.5);
  padding: 12px 24px;
  border-radius: 20px;
  backdrop-filter: blur(8px);
}

.participant-info h3 {
  margin: 0 0 4px 0;
  font-size: 18px;
  font-weight: 600;
}

.call-status {
  margin: 0 0 4px 0;
  font-size: 14px;
  opacity: 0.8;
}

.call-duration {
  margin: 0;
  font-size: 14px;
  font-weight: 500;
  color: #10b981;
}

/* Video Call UI */
.video-container {
  position: relative;
  width: 100%;
  height: 100%;
  background: #1a1a1a;
}

.remote-video {
  width: 100%;
  height: 100%;
  object-fit: cover;
  background: #000;
}

.local-video {
  position: absolute;
  top: 20px;
  right: 20px;
  width: 160px;
  height: 120px;
  border-radius: 12px;
  object-fit: cover;
  background: #333;
  border: 2px solid rgba(255, 255, 255, 0.2);
  z-index: 1000;
}

.video-placeholder {
  position: absolute;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: white;
  text-align: center;
}

.remote-placeholder {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.local-placeholder {
  top: 20px;
  right: 20px;
  width: 160px;
  height: 120px;
  border-radius: 12px;
  background: rgba(0, 0, 0, 0.7);
  border: 2px solid rgba(255, 255, 255, 0.2);
  z-index: 1000;
  font-size: 12px;
}

.participant-avatar {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  font-weight: bold;
  margin-bottom: 12px;
}

.local-placeholder .participant-avatar {
  width: 40px;
  height: 40px;
  font-size: 12px;
  margin-bottom: 8px;
}

/* Audio Call UI */
.audio-call-ui {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: white;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  text-align: center;
  padding: 40px;
}

.participant-avatar-large {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 36px;
  font-weight: bold;
  margin-bottom: 24px;
  border: 4px solid rgba(255, 255, 255, 0.3);
}

.audio-call-ui h2 {
  margin: 0 0 12px 0;
  font-size: 28px;
  font-weight: 600;
}

.call-status-large {
  margin: 0;
  font-size: 16px;
  opacity: 0.8;
}

/* Error UI */
.call-error {
  position: absolute;
  bottom: 120px;
  left: 50%;
  transform: translateX(-50%);
  background: rgba(239, 68, 68, 0.9);
  color: white;
  padding: 12px 20px;
  border-radius: 8px;
  text-align: center;
  backdrop-filter: blur(8px);
  z-index: 1001;
}

.call-error p {
  margin: 0 0 8px 0;
  font-size: 14px;
}

.clear-error-btn {
  background: rgba(255, 255, 255, 0.2);
  border: 1px solid rgba(255, 255, 255, 0.3);
  color: white;
  padding: 4px 12px;
  border-radius: 4px;
  font-size: 12px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.clear-error-btn:hover {
  background: rgba(255, 255, 255, 0.3);
}
</style>