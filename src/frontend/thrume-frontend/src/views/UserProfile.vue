<template>
  <div class="user-profile">
    <div class="profile-header">
      <div class="avatar-section">
        <img v-if="avatarUrl" :src="avatarUrl" alt="User Avatar" class="avatar-image" />
        <div v-else class="avatar-placeholder">{{ getUserInitials(username) }}</div>
        <input type="file" ref="fileInput" @change="handleFileSelected" accept="image/*" style="display: none;" />
        <button v-if="isCurrentUserProfile" @click="triggerFileInput" class="update-avatar-btn">
          Update Avatar
        </button>
      </div>
      <div class="profile-info">
        <h1>{{ username }}</h1>
        <!-- Placeholder for follower/following counts -->
        <div class="follow-stats" v-if="!isCurrentUserProfile">
           <!-- Follow/Unfollow button will go here -->
        </div>
      </div>
    </div>

    <h2>Posts</h2>
    <div v-if="loadingPosts" class="loading">Loading posts...</div>
    <div v-else-if="postsError" class="error-message">Error loading posts: {{ postsError }}</div>
    <div v-else-if="posts.length === 0" class="no-posts">No posts found for this user.</div>
    <div v-else class="posts-list">
      <!-- Use PostCard component -->
      <PostCard v-for="post in posts" :key="post.id.value" :post="post" />
    </div>
     <div v-if="uploadingAvatar" class="loading-avatar">Uploading avatar...</div>
     <div v-if="avatarUploadError" class="error-message">{{ avatarUploadError }}</div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { usePostStore } from '@/stores/postStore'; // Import postStore
import { storeToRefs } from 'pinia'; // To make store state reactive
import PostCard from '@/components/PostCard.vue';

// Define interfaces (consider moving to a shared types file)
interface PostId {
  value: string;
}
interface PostImage {
  url: string;
}
interface AuthorInfo {
    id?: string;
    username: string;
    avatarUrl?: string | null;
}
export interface Post { // Exporting for potential use elsewhere if this becomes a shared type
  id: PostId;
  content: string;
  images: PostImage[];
  likedBy: string[];
  createdAt: string;
  author: AuthorInfo;
  commentsCount?: number;
}

const route = useRoute();
const authStore = useAuthStore();
const postStore = usePostStore(); // Initialize postStore

const username = ref(route.params.username as string);

// Use reactive state from postStore
const { userPosts, isLoading: loadingPosts, error: postsError } = storeToRefs(postStore);

// Computed property to get posts for the current username
const posts = computed(() => userPosts.value[username.value] || []);

const avatarUrl = ref<string | null>(null);
const fileInput = ref<HTMLInputElement | null>(null);
const selectedFile = ref<File | null>(null);
const uploadingAvatar = ref(false);
const avatarUploadError = ref<string | null>(null);


// Determine if the currently viewed profile belongs to the logged-in user
const isCurrentUserProfile = computed(() => {
  // This assumes your authStore.currentUser.username holds the logged-in user's name
  // Or compare with a decoded token's username if available directly in authStore
  // For now, a simple check. Replace with your actual logic.
  // A more robust way would be to compare user IDs if available.
  return authStore.isAuthenticated && authStore.currentUser?.username === username.value;
});


const getUserInitials = (name: string | undefined) => {
  if (!name) return '';
  return name.substring(0, 2).toUpperCase();
};

const triggerFileInput = () => {
  fileInput.value?.click();
};

const handleFileSelected = (event: Event) => {
  const target = event.target as HTMLInputElement;
  if (target.files && target.files[0]) {
    selectedFile.value = target.files[0];
    // Optionally, display a preview of the selected image
    const reader = new FileReader();
    reader.onload = (e) => {
      // avatarUrl.value = e.target?.result as string; // Preview, might not be desired before upload
    };
    reader.readAsDataURL(selectedFile.value);
    uploadAvatar(); // Automatically upload after selection, or have a separate confirm button
  }
};

const uploadAvatar = async () => {
  if (!selectedFile.value) {
    avatarUploadError.value = "No file selected.";
    return;
  }
  if (!isCurrentUserProfile.value) {
    avatarUploadError.value = "Cannot update avatar for another user.";
    return;
  }

  uploadingAvatar.value = true;
  avatarUploadError.value = null;
  const formData = new FormData();
  formData.append('file', selectedFile.value);

  try {
    const response = await fetch(`https://localhost:5131/account/updateProfile`, {
      method: 'PUT',
      headers: {
        // 'Content-Type': 'multipart/form-data' is set automatically by browser with FormData
        // With cookie auth, the browser automatically sends the cookie, no need for Authorization header
      },
      body: formData,
    });

    if (!response.ok) {
      const errorData = await response.text();
      throw new Error(`Failed to upload avatar: ${response.status} ${errorData || response.statusText}`);
    }
    // Assuming the response is OK.
    console.log('Avatar uploaded successfully');
    selectedFile.value = null; // Clear selected file

    // IMPORTANT: The backend PUT /account/updateProfile returns Ok() but not the new avatar URL.
    // For an immediate update, we'd ideally get the new URL back.
    // Alternative strategies:
    // 1. Backend returns the new URL. (Best)
    // 2. Frontend re-fetches user profile data (if an endpoint exists that includes avatar).
    // 3. Optimistic update if the filename/path is predictable (not robust).
    // 4. For now, we can't directly update authStore.currentUser.avatarUrl without the new URL.
    //    A temporary workaround could be to read the uploaded file as a data URL for local preview,
    //    but this won't persist or reflect the actual stored URL.
    //    Let's assume for now the user might need to refresh or a future profile fetch will get it.
    //    Or, if we had a GET /account/me endpoint, we could call it.
    //    For now, we'll call fetchUserProfileData which will try to get it from the store.
    //    If the store was updated by a separate mechanism (e.g. WebSocket notification), it would reflect.
    //    A simple way is to update the local avatarUrl with a FileReader for immediate preview,
    //    and then call authStore.updateUserAvatar(newUrlFromServer) if it were available.

    // Let's try to re-read the selected file for an immediate local preview as a temporary measure.
    if (selectedFile.value) { // Check if selectedFile has a value
        const reader = new FileReader();
        reader.onload = (e) => {
            const newAvatarDataUrl = e.target?.result as string;
            avatarUrl.value = newAvatarDataUrl; // Local preview
            // If this is the current user, also update the store for consistency in this session
            if (isCurrentUserProfile.value && authStore.currentUser) {
                 authStore.updateUserAvatar(newAvatarDataUrl); // This updates the store with a local data URL
            }
        };
        reader.readAsDataURL(selectedFile.value); // Use selectedFile.value
    } else {
        // If we can't get the file again, we might need to prompt a refresh or wait for a background update.
        // For now, just log it.
        console.warn("Could not create local preview for avatar. User may need to refresh to see update from server.");
    }
    // Ideally, you would call an action in your authStore to fetch the updated user profile from the backend here.
    // authStore.fetchCurrentUserProfile(); // Example action
  } catch (err: any) {
    console.error('Avatar upload failed:', err);
    avatarUploadError.value = err.message || 'An unknown error occurred during avatar upload.';
  } finally {
    uploadingAvatar.value = false;
  }
};


// Modified fetchPosts to use the store action
const fetchPosts = async () => {
  await postStore.fetchUserPosts(username.value);
};

const fetchUserProfileData = async () => {
  // If this is the profile of the currently logged-in user,
  // their avatar URL should be in authStore.currentUser.
  if (isCurrentUserProfile.value && authStore.currentUser?.avatarUrl) {
    avatarUrl.value = authStore.currentUser.avatarUrl;
  } else if (isCurrentUserProfile.value && authStore.currentUser) {
    avatarUrl.value = null; // Or a default placeholder image path
    console.log("Current user's avatar URL not found in store. Displaying placeholder.");
  }
  else {

    console.log(`Placeholder: Need to fetch profile data for ${username.value} including avatar.`);
    // avatarUrl.value = 'default-other-user-avatar.png'; // Example placeholder
    avatarUrl.value = null; // No endpoint yet, so no avatar for others.
  }
};


onMounted(() => {
  fetchPosts();
  fetchUserProfileData();
});

// Watch for route changes if the component is reused for different profiles
watch(() => route.params.username, (newUsername) => {
  if (newUsername && typeof newUsername === 'string') {
    username.value = newUsername;
    fetchPosts();
    fetchUserProfileData();
  }
});
</script>

<style scoped>
.user-profile {
  padding: 20px;
  max-width: 800px;
  margin: 0 auto;
}

.profile-header {
  display: flex;
  align-items: center;
  margin-bottom: 30px;
  padding-bottom: 20px;
  border-bottom: 1px solid #eee;
}

.avatar-section {
  margin-right: 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.avatar-image, .avatar-placeholder {
  width: 100px;
  height: 100px;
  border-radius: 50%;
  object-fit: cover;
  border: 2px solid #ddd;
}

.avatar-placeholder {
  background-color: #007bff;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2em;
  font-weight: bold;
}

.update-avatar-btn {
  margin-top: 10px;
  padding: 6px 12px;
  font-size: 0.9em;
  background-color: #6c757d;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
.update-avatar-btn:hover {
  background-color: #5a6268;
}

.profile-info h1 {
  margin-top: 0;
  margin-bottom: 10px;
}

.follow-stats {
  /* Styles for follow stats and button */
  margin-top: 10px;
}

.posts-list {
  list-style: none;
  padding: 0;
}

.post-item {
  border: 1px solid #ccc;
  margin-bottom: 15px;
  padding: 15px;
  border-radius: 4px;
  background-color: #fff;
}

.post-images {
  margin-top: 10px;
}
.post-image {
  max-width: 100px;
  max-height: 100px;
  margin-right: 5px;
  border-radius: 4px;
  border: 1px solid #eee;
}

small {
  color: #666;
  display: block;
  margin-top: 5px;
  margin-bottom: 10px;
}

.loading-avatar, .error-message {
    margin-top: 10px;
    padding: 10px;
    border-radius: 4px;
}
.loading-avatar {
    background-color: #e9ecef;
    color: #495057;
}
.error-message {
    background-color: #f8d7da;
    color: #721c24;
    border: 1px solid #f5c6cb;
}
</style>