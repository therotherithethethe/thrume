<template>
  <div class="call-controls">
    <div class="controls-container">
      <!-- Mute/Unmute Button -->
      <button
        @click="toggleMute"
        :class="['control-btn', 'mute-btn', { active: isMuted }]"
        :title="isMuted ? 'Unmute' : 'Mute'"
      >
        <svg v-if="!isMuted" width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
          <path d="M12 2c1.1 0 2 .9 2 2v6c0 1.1-.9 2-2 2s-2-.9-2-2V4c0-1.1.9-2 2-2zm5.3 6c-.3 0-.5.2-.5.5v1.5c0 2.8-2.2 5-5 5s-5-2.2-5-5V8.5c0-.3-.2-.5-.5-.5s-.5.2-.5.5v1.5c0 3.2 2.4 5.8 5.5 6.3V18H8.5c-.3 0-.5.2-.5.5s.2.5.5.5h7c.3 0 .5-.2.5-.5s-.2-.5-.5-.5H13v-1.7c3.1-.5 5.5-3.1 5.5-6.3V8.5c0-.3-.2-.5-.5-.5z"/>
        </svg>
        <svg v-else width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
          <path d="m19 11c0 1.19-.34 2.3-.9 3.28l-1.23-1.23c.27-.62.43-1.31.43-2.05h1.7zm-4.02.17c0-.06.02-.11.02-.17v-6c0-1.1-.9-2-2-2s-2 .9-2 2v.18l3.98 3.99zm-6.98-1.67h1.7v1.5c0 .94.19 1.82.52 2.64l1.26-1.26v-1.38c0-.06-.02-.11-.02-.17l-3.46-1.33zm12.98-2.5c-.78 0-1.5.62-1.5 1.4v1.6c0 4.28-3.22 7.8-7.5 8.4v-2.7c-.78-.67-1.5-1.45-1.5-2.3 0-.19.02-.37.07-.55l-3.73-3.73c-.9.85-1.47 2.05-1.47 3.38v1.5c0-.3-.2-.5-.5-.5s-.5.2-.5.5v1.5c0 4.28 3.22 7.8 7.5 8.4v2.1h-3c-.3 0-.5.2-.5.5s.2.5.5.5h7c.3 0 .5-.2.5-.5s-.2-.5-.5-.5h-3v-2.1c.85-.12 1.64-.35 2.36-.7l2.76 2.76 1.06-1.06-17.86-17.86-1.06 1.06 3.74 3.74z"/>
        </svg>
      </button>

      <!-- Camera Toggle (Video calls only) -->
      <button
        v-if="isVideoCall"
        @click="toggleCamera"
        :class="['control-btn', 'camera-btn', { active: !isCameraOn }]"
        :title="isCameraOn ? 'Turn off camera' : 'Turn on camera'"
      >
        <svg v-if="isCameraOn" width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
          <path d="M17 10.5V7c0-.55-.45-1-1-1H4c-.55 0-1 .45-1 1v10c0 .55.45 1 1 1h12c.55 0 1-.45 1-1v-3.5l4 4v-11l-4 4z"/>
        </svg>
        <svg v-else width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
          <path d="M21 6.5l-4 4V7c0-.55-.45-1-1-1H9.82L21 17.18V6.5zM3.27 2L2 3.27L4.73 6H4c-.55 0-1 .45-1 1v10c0 .55.45 1 1 1h12c.21 0 .39-.08.54-.18L19.73 21 21 19.73 3.27 2z"/>
        </svg>
      </button>

      <!-- Screen Share Toggle (Video calls only) -->
      <button
        v-if="isVideoCall"
        @click="toggleScreenShare"
        :class="['control-btn', 'screen-share-btn', { active: isScreenSharing }]"
        :title="isScreenSharing ? 'Stop sharing' : 'Share screen'"
      >
        <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
          <path d="M20 18c1.1 0 1.99-.9 1.99-2L22 6c0-1.1-.9-2-2-2H4c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2H0v2h24v-2h-4zM4 6h16v10H4V6z"/>
        </svg>
      </button>

      <!-- End Call Button -->
      <button
        @click="endCall"
        class="control-btn end-call-btn"
        title="End call"
      >
        <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
          <path d="M12 9c-1.6 0-3.15.25-4.6.72v3.1c0 .39-.23.74-.56.9-.98.49-1.87 1.12-2.66 1.85-.18.18-.43.28-.69.28-.26 0-.51-.1-.71-.29L.29 13.08c-.18-.17-.29-.42-.29-.7 0-.28.11-.53.29-.71C3.34 8.78 7.46 7 12 7s8.66 1.78 11.71 4.67c.18.18.29.43.29.71 0 .28-.11.53-.29.7l-2.48 2.48c-.18.18-.43.29-.69.29-.26 0-.51-.1-.69-.28-.79-.73-1.68-1.36-2.66-1.85-.33-.16-.56-.5-.56-.9v-3.1C15.15 9.25 13.6 9 12 9z"/>
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useCallStore } from '../stores/callStore'
import { CallType } from '../types/signalr'

const callStore = useCallStore()

// Computed properties
const activeCall = computed(() => callStore.activeCall)
const mediaState = computed(() => callStore.mediaState)

const isVideoCall = computed(() => activeCall.value?.callType === CallType.Video)
const isMuted = computed(() => mediaState.value.isMuted)
const isCameraOn = computed(() => mediaState.value.isCameraOn)
const isScreenSharing = computed(() => mediaState.value.isScreenSharing)

// Actions
const toggleMute = async () => {
  try {
    await callStore.toggleMute()
  } catch (error) {
    console.error('Failed to toggle mute:', error)
  }
}

const toggleCamera = async () => {
  try {
    await callStore.toggleCamera()
  } catch (error) {
    console.error('Failed to toggle camera:', error)
  }
}

const toggleScreenShare = async () => {
  try {
    await callStore.toggleScreenShare()
  } catch (error) {
    console.error('Failed to toggle screen share:', error)
  }
}

const endCall = async () => {
  try {
    await callStore.endCall()
  } catch (error) {
    console.error('Failed to end call:', error)
  }
}
</script>

<style scoped>
.call-controls {
  position: absolute;
  bottom: 40px;
  left: 50%;
  transform: translateX(-50%);
  z-index: 1002;
}

.controls-container {
  display: flex;
  gap: 16px;
  padding: 16px 24px;
  background: rgba(0, 0, 0, 0.7);
  border-radius: 50px;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.control-btn {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  color: white;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
  position: relative;
}

.control-btn:hover {
  transform: scale(1.1);
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.3);
}

.control-btn:active {
  transform: scale(0.95);
}

/* Mute Button */
.mute-btn {
  background: rgba(255, 255, 255, 0.2);
  border: 2px solid rgba(255, 255, 255, 0.3);
}

.mute-btn.active {
  background: #ef4444;
  border-color: #ef4444;
}

.mute-btn:hover:not(.active) {
  background: rgba(255, 255, 255, 0.3);
}

/* Camera Button */
.camera-btn {
  background: rgba(255, 255, 255, 0.2);
  border: 2px solid rgba(255, 255, 255, 0.3);
}

.camera-btn.active {
  background: #ef4444;
  border-color: #ef4444;
}

.camera-btn:hover:not(.active) {
  background: rgba(255, 255, 255, 0.3);
}

/* Screen Share Button */
.screen-share-btn {
  background: rgba(255, 255, 255, 0.2);
  border: 2px solid rgba(255, 255, 255, 0.3);
}

.screen-share-btn.active {
  background: #10b981;
  border-color: #10b981;
}

.screen-share-btn:hover:not(.active) {
  background: rgba(255, 255, 255, 0.3);
}

/* End Call Button */
.end-call-btn {
  background: #ef4444;
  border: 2px solid #ef4444;
}

.end-call-btn:hover {
  background: #dc2626;
  border-color: #dc2626;
}

/* Active state indicator */
.control-btn.active::after {
  content: '';
  position: absolute;
  top: -4px;
  right: -4px;
  width: 12px;
  height: 12px;
  background: #10b981;
  border-radius: 50%;
  border: 2px solid white;
}

.mute-btn.active::after,
.camera-btn.active::after {
  background: #ef4444;
}

.screen-share-btn.active::after {
  background: #10b981;
}
</style>