<template>
  <div class="user-presence-indicator">
    <div 
      class="presence-dot" 
      :class="presenceClass"
      :title="presenceTitle"
    ></div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useMessageStore } from '../stores/messageStore'

interface Props {
  userId: string
  showOffline?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  showOffline: false
})

const messageStore = useMessageStore()

const userPresence = computed(() => {
  return messageStore.userPresences.get(props.userId)
})

const isOnline = computed(() => {
  return userPresence.value?.isOnline ?? false
})

const presenceClass = computed(() => {
  if (isOnline.value) {
    return 'presence-online'
  } else if (props.showOffline) {
    return 'presence-offline'
  }
  return 'presence-hidden'
})

const presenceTitle = computed(() => {
  if (isOnline.value) {
    return 'Online'
  } else if (userPresence.value?.lastSeen) {
    const lastSeen = new Date(userPresence.value.lastSeen)
    const now = new Date()
    const diffMs = now.getTime() - lastSeen.getTime()
    const diffMinutes = Math.floor(diffMs / (1000 * 60))
    const diffHours = Math.floor(diffMinutes / 60)
    const diffDays = Math.floor(diffHours / 24)
    
    if (diffMinutes < 1) {
      return 'Last seen just now'
    } else if (diffMinutes < 60) {
      return `Last seen ${diffMinutes} minute${diffMinutes === 1 ? '' : 's'} ago`
    } else if (diffHours < 24) {
      return `Last seen ${diffHours} hour${diffHours === 1 ? '' : 's'} ago`
    } else {
      return `Last seen ${diffDays} day${diffDays === 1 ? '' : 's'} ago`
    }
  }
  return 'Offline'
})
</script>

<style scoped>
.user-presence-indicator {
  display: inline-block;
}

.presence-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  border: 2px solid white;
  position: relative;
}

.presence-online {
  background-color: #28a745;
  animation: pulse-online 2s ease-in-out infinite;
}

.presence-offline {
  background-color: #6c757d;
}

.presence-hidden {
  display: none;
}

@keyframes pulse-online {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.7;
  }
}

/* Small variant for inline use */
.user-presence-indicator.small .presence-dot {
  width: 8px;
  height: 8px;
  border-width: 1px;
}
</style>