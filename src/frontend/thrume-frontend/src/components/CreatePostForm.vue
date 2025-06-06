<template>
  <div class="modal-backdrop" @click.self="$emit('close')">
    <div class="create-post-modal">
      <button class="close-button" @click="$emit('close')">×</button>
      <h2>Create Post</h2>
      <textarea v-model="content" maxlength="200" placeholder="What's on your mind?"></textarea>
      <div class="char-counter">{{ content.length }}/200</div>
      
      <label class="file-upload">
        <span>Select Images</span>
        <input type="file" multiple accept="image/*" @change="handleImageUpload">
      </label>
      
      <div class="image-previews">
        <div v-for="(image, index) in images" :key="index" class="image-preview">
          <img :src="getImageUrl(image)" alt="Preview">
          <button @click="removeImage(index)">×</button>
        </div>
      </div>
      
      <div class="actions">
        <button @click="$emit('close')">Cancel</button>
        <button @click="submitPost" :disabled="isSubmitting">Post</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { createPost } from '../services/postService';
import { CreatePostRequest } from '../types';

const content = ref('');
const images = ref<File[]>([]);
const isSubmitting = ref(false);

function handleImageUpload(e: Event) {
  const files = (e.target as HTMLInputElement).files;
  if (files) {
    const newImages = Array.from(files).slice(0, 10 - images.value.length);
    images.value = [...images.value, ...newImages];
  }
}

function removeImage(index: number) {
  images.value.splice(index, 1);
}

async function submitPost() {
    if (content.value.trim() === '' && images.value.length == 0) return;
    
  isSubmitting.value = true;
  try {
    await createPost({ content: content.value, images: images.value });
    content.value = '';
    images.value = [];
    emit('close');
  } catch (error) {
    console.error('Post creation failed:', error);
  } finally {
    isSubmitting.value = false;
  }
}

function getImageUrl(image: File) {
  return window.URL.createObjectURL(image);
}

const emit = defineEmits(['close']);
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

.create-post-modal {
  position: relative;
  background: white;
  padding: 30px 20px 20px;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  z-index: 1000;
  width: 500px;
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
}

h2 {
  color: #333;
  margin-top: 0;
}

textarea {
  width: 100%;
  height: 100px;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
  resize: vertical;
  color: #333;
}

.char-counter {
  text-align: right;
  font-size: 0.8rem;
  color: #666;
  margin-top: 5px;
}

.file-upload {
  display: block;
  margin: 15px 0;
  padding: 8px 12px;
  background: #f0f0f0;
  border-radius: 4px;
  cursor: pointer;
  color: #333;
}

.file-upload span {
  margin-right: 10px;
}

.file-upload input[type="file"] {
  display: none;
}

.image-previews {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-bottom: 15px;
  max-height: 200px;
  overflow-y: auto;
}

.image-preview {
  position: relative;
  width: 120px;
  height: 120px;
  flex-shrink: 0;
}

.image-preview img {
  width: 100%;
  height: 100%;
  object-fit: contain;
  border-radius: 4px;
  background: #f0f0f0;
}

.image-preview button {
  position: absolute;
  top: -10px;
  right: -10px;
  background: white;
  border: 1px solid #ddd;
  border-radius: 50%;
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: #333;
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
}

button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

button:last-child {
  background-color: #3498db;
  color: white;
}
</style>