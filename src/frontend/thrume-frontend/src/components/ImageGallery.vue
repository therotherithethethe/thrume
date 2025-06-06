<template>
  <div class="gallery" v-if="open" @click.self="close">
    <button class="close-btn" @click="close">✕</button>
    <div class="gallery-content">
      <transition :name="transitionName">
        <img :key="currentIndex" :src="currentImage" class="gallery-image" 
             @touchstart="handleTouchStart"
             @touchmove="handleTouchMove"
             @touchend="handleTouchEnd" />
      </transition>
      <button class="nav-btn prev" @click="prev">❮</button>
      <button class="nav-btn next" @click="next">❯</button>
    </div>
    <div class="gallery-index">{{ currentIndex + 1 }}/{{ images.length }}</div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, PropType } from 'vue';

const props = defineProps({
  images: {
    type: Array as PropType<string[]>,
    required: true,
    default: () => []
  },
  initialIndex: {
    type: Number,
    default: 0
  },
  open: {
    type: Boolean,
    required: true
  }
});

const emit = defineEmits(['close']);

const currentIndex = ref(props.initialIndex || 0);
const currentImage = computed(() => {
  return props.images[currentIndex.value] || '';
});
const transitionName = ref('slide-next');

// Touch handling for swipe gestures
const touchStartX = ref(0);
const touchStartY = ref(0);
const touchEndX = ref(0);

function handleTouchStart(e: TouchEvent) {
  touchStartX.value = e.touches[0].clientX;
  touchStartY.value = e.touches[0].clientY;
}

function handleTouchMove(e: TouchEvent) {
  touchEndX.value = e.touches[0].clientX;
}

function handleTouchEnd() {
  const diffX = touchStartX.value - touchEndX.value;
  const absDiffX = Math.abs(diffX);
  
  // Only consider horizontal swipes with significant movement
  if (absDiffX > 50) {
    if (diffX > 0) {
      next();
    } else {
      prev();
    }
  }
}

function next() {
  if (props.images.length === 0) return;
  
  transitionName.value = 'slide-next';
  currentIndex.value = (currentIndex.value + 1) % props.images.length;
}

function prev() {
  if (props.images.length === 0) return;
  
  transitionName.value = 'slide-prev';
  currentIndex.value = (currentIndex.value - 1 + props.images.length) % props.images.length;
}

function close() {
  emit('close');
}

// Keyboard navigation
function handleKeydown(e: KeyboardEvent) {
  if (e.key === 'ArrowLeft') prev();
  if (e.key === 'ArrowRight') next();
  if (e.key === 'Escape') close();
}

// Add keyboard event listeners when gallery is open
watch(() => props.open, (isOpen) => {
  if (isOpen) {
    document.addEventListener('keydown', handleKeydown);
  } else {
    document.removeEventListener('keydown', handleKeydown);
  }
});
</script>

<style scoped>
.gallery {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0,0,0,0.95);
  z-index: 1000;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
}

.close-btn {
  position: absolute;
  top: 20px;
  right: 20px;
  background: none;
  border: none;
  color: white;
  font-size: 2rem;
  cursor: pointer;
  z-index: 1001;
}

.gallery-content {
  position: relative;
  width: 100%;
  height: 80vh;
  display: flex;
  justify-content: center;
  align-items: center;
}

.gallery-image {
  max-width: 90%;
  max-height: 90%;
  object-fit: contain;
}

.nav-btn {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  background: rgba(0,0,0,0.5);
  border: none;
  color: white;
  font-size: 2rem;
  width: 50px;
  height: 50px;
  border-radius: 50%;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1001;
}

.prev { left: 20px; }
.next { right: 20px; }

.gallery-index {
  color: white;
  text-align: center;
  margin-top: 20px;
  font-size: 1.2rem;
}

/* Slide transitions */


.slide-next-enter-from {
  transform: translateX(100%);
}

.slide-next-leave-to {
  transform: translateX(-100%);
}

.slide-prev-enter-from {
  transform: translateX(-100%);
}

.slide-prev-leave-to {
  transform: translateX(100%);
}
</style>