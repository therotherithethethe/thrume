<template>
  <div v-if="isOpen" class="modal-backdrop" @click.self="handleBackdropClick">
    <div class="delete-confirmation-modal" @click.stop ref="modalRef">
      <button class="close-button" @click="handleCancel" :disabled="isLoading" aria-label="Close modal">×</button>
      
      <!-- Header -->
      <h2>{{ title }}</h2>
      
      <!-- Body -->
      <div class="warning-content">
        <div class="warning-icon">
          <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
          </svg>
        </div>
        <p class="warning-message">{{ message }}</p>
      </div>
      
      <!-- Footer -->
      <div class="actions">
        <button @click="handleCancel" :disabled="isLoading" ref="cancelButtonRef" class="cancel-button">
          {{ cancelText }}
        </button>
        <button @click="handleConfirm" :disabled="isLoading" class="confirm-button">
          <span v-if="isLoading" class="spinner"></span>
          <span>{{ confirmText }}</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick, watch, onMounted, onUnmounted } from 'vue'

interface Props {
  isOpen: boolean
  title?: string
  message?: string
  confirmText?: string
  cancelText?: string
  isLoading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  title: 'Delete Conversation',
  message: 'Are you sure you want to delete this conversation? This action cannot be undone and all messages will be permanently lost.',
  confirmText: 'Delete',
  cancelText: 'Cancel',
  isLoading: false
})

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()

const modalRef = ref<HTMLDivElement>()
const cancelButtonRef = ref<HTMLButtonElement>()

// Handle backdrop click
const handleBackdropClick = () => {
  if (!props.isLoading) {
    handleCancel()
  }
}

// Handle cancel action
const handleCancel = () => {
  if (!props.isLoading) {
    emit('cancel')
  }
}

// Handle confirm action
const handleConfirm = () => {
  if (!props.isLoading) {
    emit('confirm')
  }
}

// Keyboard event handler
const handleKeydown = (event: KeyboardEvent) => {
  if (!props.isOpen) return

  switch (event.key) {
    case 'Escape':
      event.preventDefault()
      handleCancel()
      break
    case 'Enter':
      event.preventDefault()
      handleConfirm()
      break
  }
}

// Focus management
const focusModal = async () => {
  await nextTick()
  if (cancelButtonRef.value) {
    cancelButtonRef.value.focus()
  }
}

// Watch for modal open/close
watch(
  () => props.isOpen,
  (isOpen) => {
    if (isOpen) {
      focusModal()
      document.body.style.overflow = 'hidden'
    } else {
      document.body.style.overflow = ''
    }
  }
)

// Lifecycle hooks
onMounted(() => {
  document.addEventListener('keydown', handleKeydown)
  if (props.isOpen) {
    focusModal()
    document.body.style.overflow = 'hidden'
  }
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleKeydown)
  document.body.style.overflow = ''
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

.delete-confirmation-modal {
  position: relative;
  background: white;
  padding: 30px 20px 20px;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  z-index: 1000;
  width: 420px;
  max-width: 90%;
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
  transition: color 0.2s;
}

.close-button:hover:not(:disabled) {
  color: #666;
}

.close-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

h2 {
  color: #333;
  margin-top: 0;
  margin-bottom: 20px;
  text-align: center;
  font-size: 1.25rem;
}

.warning-content {
  display: flex;
  align-items: flex-start;
  gap: 15px;
  margin-bottom: 25px;
}

.warning-icon {
  flex-shrink: 0;
  color: #e74c3c;
  margin-top: 2px;
}

.warning-message {
  flex: 1;
  color: #333;
  line-height: 1.5;
  margin: 0;
}

.actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

button {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  gap: 6px;
}

button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.cancel-button {
  background-color: #95a5a6;
  color: white;
}

.cancel-button:hover:not(:disabled) {
  background-color: #7f8c8d;
}

.confirm-button {
  background-color: #e74c3c;
  color: white;
}

.confirm-button:hover:not(:disabled) {
  background-color: #c0392b;
}

.spinner {
  width: 14px;
  height: 14px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: white;
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to { 
    transform: rotate(360deg); 
  }
}

/* Focus styles for accessibility */
button:focus {
  outline: 2px solid #3498db;
  outline-offset: 2px;
}

.cancel-button:focus {
  outline-color: #95a5a6;
}

.confirm-button:focus {
  outline-color: #e74c3c;
}
</style>