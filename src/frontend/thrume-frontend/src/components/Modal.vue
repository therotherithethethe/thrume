<template>
  <transition name="modal-fade">
    <div v-if="isOpen" class="modal-overlay" @click.self="closeModal">
      <div class="modal-container" role="dialog" aria-modal="true" :aria-labelledby="titleId">
        <header class="modal-header">
          <h2 :id="titleId" class="modal-title">{{ title }}</h2>
          <button @click="closeModal" class="close-button" aria-label="Close modal">&times;</button>
        </header>
        <section class="modal-body">
          <slot></slot> <!-- Content of the modal will go here -->
        </section>
        <footer class="modal-footer" v-if="$slots.footer">
          <slot name="footer"></slot>
        </footer>
      </div>
    </div>
  </transition>
</template>

<script setup lang="ts">
import { defineProps, defineEmits, computed } from 'vue';

const props = defineProps<{
  isOpen: boolean;
  title?: string;
}>();

const emit = defineEmits(['close']);

const closeModal = () => {
  emit('close');
};

// Generate a unique ID for aria-labelledby
const titleId = computed(() => `modal-title-${Math.random().toString(36).substring(2, 9)}`);

// Optional: Handle Escape key to close modal
import { onMounted, onUnmounted } from 'vue';

const handleEsc = (event: KeyboardEvent) => {
  if (event.key === 'Escape' && props.isOpen) {
    closeModal();
  }
};

onMounted(() => {
  document.addEventListener('keydown', handleEsc);
});

onUnmounted(() => {
  document.removeEventListener('keydown', handleEsc);
});
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.6);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000; /* Ensure it's on top */
}

.modal-container {
  background-color: #fff;
  border-radius: 8px;
  box-shadow: 0 5px 15px rgba(0,0,0,0.3);
  width: auto;
  max-width: 90%; /* Max width for responsiveness */
  min-width: 300px; /* Min width */
  max-height: 90vh; /* Max height */
  display: flex;
  flex-direction: column;
  overflow: hidden; /* Prevent content from spilling out before scrollbars appear */
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px 20px;
  border-bottom: 1px solid #eee;
}

.modal-title {
  margin: 0;
  font-size: 1.25em;
  font-weight: 600;
}

.close-button {
  background: none;
  border: none;
  font-size: 1.8em;
  cursor: pointer;
  color: #888;
  padding: 0;
  line-height: 1;
}
.close-button:hover {
  color: #333;
}

.modal-body {
  padding: 20px;
  overflow-y: auto; /* Allow body to scroll if content is too long */
  flex-grow: 1;
}

.modal-footer {
  padding: 15px 20px;
  border-top: 1px solid #eee;
  display: flex;
  justify-content: flex-end;
}

/* Transition effects */
.modal-fade-enter-active, .modal-fade-leave-active {
  transition: opacity 0.3s ease;
}
.modal-fade-enter-from, .modal-fade-leave-to {
  opacity: 0;
}

.modal-fade-enter-active .modal-container,
.modal-fade-leave-active .modal-container {
  transition: transform 0.3s ease;
}
.modal-fade-enter-from .modal-container,
.modal-fade-leave-to .modal-container {
  transform: scale(0.95);
}
</style>