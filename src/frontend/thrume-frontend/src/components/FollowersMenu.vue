<template>
  <div v-if="isOpen" class="modal-backdrop" @click.self="$emit('close')">
    <div class="followers-modal">
      <button class="close-button" @click="$emit('close')">×</button>
      <h2>{{ type === 'followers' ? 'Followers' : 'Following' }}</h2>
      
      <!-- Loading state -->
      <div v-if="loading" class="loading-container">
        <div class="spinner"></div>
      </div>
      
      <!-- Error state -->
      <div v-else-if="error" class="error-container">
        <p class="error-message">{{ error }}</p>
        <button @click="fetchUsers" class="retry-button">Try Again</button>
      </div>
      
      <!-- Users list -->
      <div v-else-if="users.length > 0" class="users-list">
        <div
          v-for="user in users"
          :key="user.userName"
          class="user-item clickable"
          @click="navigateToProfile(user)"
        >
          <!-- User avatar -->
          <img
            v-if="user.pictureUrl"
            :src="user.pictureUrl"
            :alt="`${user.userName}'s avatar`"
            class="user-avatar"
            @error="handleImageError"
          />
          <div v-else class="user-avatar placeholder-avatar">
            <svg width="20" height="20" viewBox="0 0 24 24">
              <circle cx="12" cy="12" r="10" fill="#e0e0e0"/>
              <path d="M12 12m-4 0a4 4 0 1 0 8 0a4 4 0 1 0 -8 0" fill="#9e9e9e"/>
            </svg>
          </div>
          
          <!-- User info -->
          <div class="user-info">
            <span class="username">{{ user.userName }}</span>
          </div>
        </div>
      </div>
      
      <!-- Empty state -->
      <div v-else class="empty-state">
        <p>{{ type === 'followers' ? 'No followers yet' : 'Not following anyone yet' }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import accountService, { FollowerFollowingUser } from '../services/accountService'

// Props
interface Props {
  isOpen: boolean
  type: 'followers' | 'following'
  userName: string
}

const props = defineProps<Props>()

// Emits
const emit = defineEmits<{
  close: []
}>()

// Router
const router = useRouter()

// State
const users = ref<FollowerFollowingUser[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

// Methods
const fetchUsers = async () => {
  if (!props.userName) return

  loading.value = true
  error.value = null

  try {
    if (props.type === 'followers') {
      users.value = await accountService.getFollowers(props.userName)
    } else {
      users.value = await accountService.getFollowing(props.userName)
    }
  } catch (err) {
    console.error(`Error fetching ${props.type}:`, err)
    error.value = `Failed to load ${props.type}. Please try again.`
  } finally {
    loading.value = false
  }
}

const handleImageError = (event: Event) => {
  const img = event.target as HTMLImageElement
  // Hide the broken image and show placeholder
  img.style.display = 'none'
  const placeholder = img.nextElementSibling as HTMLElement
  if (placeholder && placeholder.classList.contains('placeholder-avatar')) {
    placeholder.style.display = 'flex'
  }
}

const navigateToProfile = (user: FollowerFollowingUser) => {
  router.push(`/${user.userName}`)
  emit('close')
}

const handleEscapeKey = (event: KeyboardEvent) => {
  if (event.key === 'Escape' && props.isOpen) {
    emit('close')
  }
}

// Watchers
watch(() => props.isOpen, (isOpen) => {
  if (isOpen) {
    fetchUsers()
  }
})

watch(() => [props.type, props.userName], () => {
  if (props.isOpen) {
    fetchUsers()
  }
})

// Lifecycle
onMounted(() => {
  document.addEventListener('keydown', handleEscapeKey)
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleEscapeKey)
})
</script>

<style scoped>
.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 999;
}

.followers-modal {
  position: relative;
  background: white;
  padding: 30px 20px 20px;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  z-index: 1000;
  width: 400px;
  max-width: 90%;
  max-height: 80vh;
  display: flex;
  flex-direction: column;
}

.close-button {
  position: absolute;
  top: 10px;
  right: 10px;
  background: none;
  border: none;
  font-size: 24px;
  cursor: pointer;
  color: #333;
}

.close-button:hover {
  color: #666;
}

h2 {
  color: #333;
  margin-top: 0;
  margin-bottom: 20px;
  text-align: center;
}

.loading-container {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 40px 0;
}

.spinner {
  width: 30px;
  height: 30px;
  border: 3px solid rgba(0, 0, 0, 0.1);
  border-radius: 50%;
  border-top-color: #3498db;
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.error-container {
  text-align: center;
  padding: 20px 0;
}

.error-message {
  color: #e74c3c;
  margin-bottom: 15px;
}

.retry-button {
  padding: 8px 16px;
  background-color: #3498db;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.retry-button:hover {
  background-color: #2980b9;
}

.users-list {
  flex: 1;
  overflow-y: auto;
  max-height: 400px;
}

.user-item {
  display: flex;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid #f0f0f0;
  transition: background-color 0.2s;
}

.user-item.clickable {
  cursor: pointer;
  padding: 10px 8px;
  margin: 0 -8px;
  border-radius: 4px;
  transition: all 0.2s ease;
}

.user-item.clickable:hover {
  background-color: #f0f8ff;
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.user-item:hover {
  background-color: #f9f9f9;
}

.user-item:last-child {
  border-bottom: none;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  object-fit: cover;
  margin-right: 12px;
  border: 1px solid #eee;
  flex-shrink: 0;
}

.placeholder-avatar {
  display: flex;
  justify-content: center;
  align-items: center;
  background: #f5f5f5;
}

.user-info {
  flex: 1;
  min-width: 0;
}

.username {
  font-weight: 500;
  color: #333;
  font-size: 14px;
}

.empty-state {
  text-align: center;
  padding: 40px 20px;
  color: #666;
}

.empty-state p {
  margin: 0;
  font-style: italic;
}
</style>