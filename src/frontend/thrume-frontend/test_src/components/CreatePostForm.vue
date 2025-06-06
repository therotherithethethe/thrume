<template>
  <div class="create-post-form">
    <h2>Create New Post</h2>
    <form @submit.prevent="submitPost">
      <div class="form-group">
        <label for="post-content">What's on your mind?</label>
        <textarea
          id="post-content"
          v-model="content"
          rows="4"
          placeholder="Write your post content here..."
          required
        ></textarea>
      </div>

      <div class="form-group">
        <label for="post-images">Add Images (Optional)</label>
        <input
          type="file"
          id="post-images"
          ref="fileInput"
          multiple
          accept="image/*"
          @change="handleFilesSelected"
        />
        <div v-if="selectedFiles.length > 0" class="image-preview-container">
          <p>Selected Images:</p>
          <div class="image-previews">
            <div v-for="(file, index) in selectedFiles" :key="index" class="image-preview">
              <img :src="getFilePreviewUrl(file)" :alt="file.name" />
              <span>{{ file.name }}</span>
              <button type="button" @click="removeSelectedFile(index)" class="remove-btn">&times;</button>
            </div>
          </div>
        </div>
      </div>

      <div class="form-actions">
        <button type="submit" :disabled="isSubmitting || !content.trim()">
          {{ isSubmitting ? 'Posting...' : 'Post' }}
        </button>
        <button type="button" @click="cancelPost" class="cancel-btn">Cancel</button>
      </div>
      <div v-if="error" class="error-message">{{ error }}</div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, defineEmits } from 'vue';
import { usePostStore } from '@/stores/postStore'; // Import postStore

const content = ref('');
const selectedFiles = ref<File[]>([]);
const fileInput = ref<HTMLInputElement | null>(null);
const isSubmitting = ref(false);
const error = ref<string | null>(null);

const postStore = usePostStore(); // Initialize postStore

const emit = defineEmits(['close', 'post-created']);

const handleFilesSelected = (event: Event) => {
  const target = event.target as HTMLInputElement;
  if (target.files) {
    // Append new files to the existing list, prevent duplicates if needed
    const newFiles = Array.from(target.files);
    // Basic check to avoid adding the exact same file object again if input is clicked multiple times
    newFiles.forEach(newFile => {
        if (!selectedFiles.value.some(existingFile => existingFile.name === newFile.name && existingFile.size === newFile.size && existingFile.lastModified === newFile.lastModified)) {
            selectedFiles.value.push(newFile);
        }
    });
    // Clear the input value so the change event fires again if the same file is selected after removal
    if (fileInput.value) {
        fileInput.value.value = '';
    }
  }
};

const getFilePreviewUrl = (file: File): string => {
  // Create a temporary URL for image preview
  return URL.createObjectURL(file);
};

const removeSelectedFile = (index: number) => {
  selectedFiles.value.splice(index, 1);
};

const submitPost = async () => {
  if (!content.value.trim() || isSubmitting.value) {
    return;
  }

  isSubmitting.value = true;
  error.value = null;

  const formData = new FormData();
  formData.append('content', content.value.trim()); // Assuming 'content' is the field name for text
  selectedFiles.value.forEach((file) => {
    formData.append('images', file); // Assuming 'images' is the field name for files
  });

  console.log('Submitting post...');
  // Log FormData contents (for debugging, won't show files directly in console)
  for (let [key, value] of formData.entries()) {
    console.log(`${key}:`, value);
  }

  try {
    await postStore.createPost(formData); // Call the store action

    // Reset form and emit success
    content.value = '';
    selectedFiles.value = [];
    if (fileInput.value) fileInput.value.value = ''; // Clear file input
    emit('post-created');
    emit('close'); // Close the form/modal
  } catch (err: any) {
    console.error('Failed to create post:', err);
    // The error should already be set in the store, but we can also set it locally if needed
    // or rely on a global error display mechanism that watches store.error.
    error.value = postStore.error || err.message || 'Failed to create post. Please try again.';
  } finally {
    isSubmitting.value = false;
  }
};

const cancelPost = () => {
  // Reset form state if needed
  content.value = '';
  selectedFiles.value = [];
  error.value = null;
  emit('close'); // Emit event to close the form/modal
};

// Clean up object URLs when component is unmounted (though less critical for short-lived previews)
// import { onUnmounted } from 'vue';
// onUnmounted(() => {
//   selectedFiles.value.forEach(file => URL.revokeObjectURL(getFilePreviewUrl(file)));
// });
</script>

<style scoped>
.create-post-form {
  background-color: #fff;
  padding: 20px 30px;
  border-radius: 8px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
  max-width: 600px;
  margin: 20px auto; /* Center if used as a page component */
}

h2 {
  text-align: center;
  margin-bottom: 20px;
  color: #333;
}

.form-group {
  margin-bottom: 20px;
}

label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #555;
}

textarea {
  width: 100%;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 4px;
  font-size: 1em;
  line-height: 1.5;
  resize: vertical; /* Allow vertical resize */
}

input[type="file"] {
  display: block;
  margin-top: 5px;
}

.image-preview-container {
    margin-top: 15px;
    padding: 10px;
    background-color: #f9f9f9;
    border: 1px dashed #ddd;
    border-radius: 4px;
}

.image-previews {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
    margin-top: 10px;
}

.image-preview {
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    max-width: 100px;
}

.image-preview img {
    width: 80px;
    height: 80px;
    object-fit: cover;
    border-radius: 4px;
    margin-bottom: 5px;
}

.image-preview span {
    font-size: 0.8em;
    color: #555;
    text-align: center;
    word-break: break-all;
    max-width: 80px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.remove-btn {
    position: absolute;
    top: -5px;
    right: -5px;
    background-color: rgba(255, 0, 0, 0.7);
    color: white;
    border: none;
    border-radius: 50%;
    width: 20px;
    height: 20px;
    font-size: 1em;
    line-height: 18px;
    text-align: center;
    cursor: pointer;
    padding: 0;
}
.remove-btn:hover {
    background-color: rgba(255, 0, 0, 1);
}


.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 20px;
}

button {
  padding: 10px 20px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1em;
  transition: background-color 0.2s;
}

button[type="submit"] {
  background-color: #007bff;
  color: white;
}

button[type="submit"]:disabled {
  background-color: #a0cfff;
  cursor: not-allowed;
}

button[type="submit"]:hover:not(:disabled) {
  background-color: #0056b3;
}

.cancel-btn {
  background-color: #f8f9fa;
  color: #333;
  border: 1px solid #ccc;
}

.cancel-btn:hover {
  background-color: #e2e6ea;
}

.error-message {
  color: red;
  margin-top: 15px;
  text-align: center;
  background-color: #f8d7da;
  padding: 10px;
  border: 1px solid #f5c6cb;
  border-radius: 4px;
}
</style>