<template>
  <div class="post-card">
    <div class="post-header">
      <div class="author-info">
        <!-- Avatar Placeholder: Ideally, fetch author's avatar -->
        <div class="avatar-placeholder">{{ getAuthorInitials(post.author?.username) }}</div>
        <span class="author-name">{{ post.author?.username || 'Unknown Author' }}</span>
      </div>
      <small class="post-timestamp">{{ formatTimestamp(post.createdAt) }}</small>
    </div>

    <div class="post-content">
      <p>{{ post.content }}</p>
    </div>

    <div v-if="post.images && post.images.length > 0" class="post-images">
      <img v-for="(image, index) in post.images" :key="index" :src="image.url" :alt="`Post image ${index + 1}`" class="post-image" @error="onImageError" />
    </div>

    <div class="post-actions">
      <button @click="toggleLike" class="action-btn like-btn">
        <span v-if="isLikedByCurrentUser">❤️ Liked</span>
        <span v-else>🤍 Like</span>
        ({{ post.likedBy?.length || 0 }})
      </button>
      <button @click="toggleComments" class="action-btn comment-btn">
        💬 Comment ({{ post.commentsCount || 0 }}) <!-- Assuming commentsCount will be added -->
      </button>
      <!-- Share button can be added later -->
    </div>

    <!-- Placeholder for comments section, to be shown when toggled -->
    <div v-if="showCommentsSection" class="comments-section">
      <p><em>Comments section will appear here.</em></p>
      <!-- <CommentList :postId="post.id.value" /> -->
      <!-- <CreateCommentForm :postId="post.id.value" /> -->
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, defineProps } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { usePostStore } from '@/stores/postStore'; // Import postStore

// Define the structure of a Post, mirroring UserProfile.vue and backend response
interface PostId {
  value: string;
}
interface PostImage {
  url: string; // Or whatever structure your image objects have
}
interface AuthorInfo { // Basic author info, expand as needed
    id?: string; // Made ID optional as it might not always be available
    username: string;
    avatarUrl?: string | null;
}
interface Post {
  id: PostId;
  content: string;
  images: PostImage[];
  likedBy: string[]; // Assuming likedBy is an array of user IDs (strings)
  createdAt: string;
  author: AuthorInfo; // Assuming post object will include author info
  commentsCount?: number; // Optional: if backend provides comment count directly
}

const props = defineProps<{
  post: Post;
}>();

const authStore = useAuthStore();
const postStore = usePostStore(); // Initialize postStore

const showCommentsSection = ref(false);

const isLikedByCurrentUser = computed(() => {
  if (!authStore.currentUser || !props.post.likedBy) {
    return false;
  }
  return props.post.likedBy.includes(authStore.currentUser.id); // Compare with current user's ID
});

const getAuthorInitials = (name: string | undefined) => {
  if (!name) return '??';
  return name.substring(0, 2).toUpperCase();
};

const formatTimestamp = (timestamp: string) => {
  if (!timestamp) return '';
  return new Date(timestamp).toLocaleString();
};

const toggleLike = async () => {
  if (!authStore.isAuthenticated) {
    // Maybe redirect to login or show a message
    console.warn("User not authenticated. Cannot like post.");
    return;
  }
  // The `togglePostLike` action in the store handles optimistic updates and API calls.
  // We need to pass the username of the post's author if this card is on a user profile page,
  // so the store knows which list of `userPosts` to update.
  // If the card is on a general feed, this might be undefined.
  const authorUsername = props.post.author?.username;
  await postStore.togglePostLike(props.post.id.value, authorUsername);
};

const toggleComments = () => {
  showCommentsSection.value = !showCommentsSection.value;
  console.log(`Toggling comments for post ID: ${props.post.id.value}`);
};

const onImageError = (event: Event) => {
  (event.target as HTMLImageElement).src = 'https://via.placeholder.com/150/FF0000/FFFFFF?Text=Error'; // Fallback image
};

</script>

<style scoped>
.post-card {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  background-color: #ffffff;
  padding: 15px;
  margin-bottom: 20px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
}

.post-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.author-info {
  display: flex;
  align-items: center;
}

.avatar-placeholder {
  width: 35px;
  height: 35px;
  border-radius: 50%;
  background-color: #007bff;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.9em;
  font-weight: bold;
  margin-right: 8px;
}

.author-name {
  font-weight: 600;
  font-size: 0.95em;
}

.post-timestamp {
  font-size: 0.8em;
  color: #6c757d;
}

.post-content p {
  margin-top: 0;
  margin-bottom: 10px;
  line-height: 1.6;
  white-space: pre-wrap; /* Preserve line breaks and spaces */
}

.post-images {
  display: flex;
  flex-wrap: wrap;
  gap: 5px;
  margin-bottom: 10px;
}

.post-image {
  max-width: 100px;
  max-height: 100px;
  border-radius: 4px;
  object-fit: cover;
  border: 1px solid #eee;
}

.post-actions {
  display: flex;
  gap: 10px;
  padding-top: 10px;
  border-top: 1px solid #f0f0f0;
}

.action-btn {
  background: none;
  border: 1px solid #ddd;
  border-radius: 20px;
  padding: 6px 12px;
  cursor: pointer;
  font-size: 0.85em;
  color: #333;
  transition: background-color 0.2s, color 0.2s;
}

.action-btn:hover {
  background-color: #f5f5f5;
}

.like-btn span {
  margin-right: 4px;
}

.comments-section {
  margin-top: 15px;
  padding-top: 10px;
  border-top: 1px dashed #eee;
}
</style>