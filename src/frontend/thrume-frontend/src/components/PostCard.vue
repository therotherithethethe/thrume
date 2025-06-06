<template>
  <article class="post-card">
    <ImageGallery
      v-if="galleryOpen"
      :images="post.images"
      :initial-index="galleryIndex"
      :open="galleryOpen"
      @close="galleryOpen = false"
    />
    <header class="post-header">
      <img
        v-if="post.author?.profilePictureUrl"
        :src="post.author.profilePictureUrl"
        :alt="`${post.author.username} profile picture`"
        class="author-avatar"
      />
      <div class="author-info">
        <span class="author-username" @click="navigateTo(post.author.username)">{{
          post.author?.username || 'Unknown User'
        }}</span>
        <time :datetime="post.createdAt">{{ formattedDate }}</time>
      </div>

      <div class="post-actions-menu">
        <button class="menu-button" @click.stop="showMenu = !showMenu">⋮</button>
        <div class="menu-dropdown" v-if="showMenu" v-click-outside="closeMenu">
          <button v-if="isPostOwner" class="delete-button" @click.stop="confirmDelete">
            Delete
          </button>
          <button v-else class="menu-item" @click.stop>Report</button>
        </div>
      </div>
    </header>

    <div v-if="showDeleteDialog" class="dialog-backdrop">
      <div class="confirmation-dialog">
        <p>Are you sure you want to delete this post?</p>
        <div class="dialog-buttons">
          <button @click="showDeleteDialog = false">Cancel</button>
          <button class="confirm-delete" @click="deletePost">Delete</button>
        </div>
      </div>
    </div>

    <p class="post-content">{{ post.content }}</p>

    <div class="post-images" :class="imageGridClass" v-if="hasImages">
      <div
        v-for="(imageUrl, index) in displayedImages"
        :key="imageUrl"
        class="image-container"
        @click="openGallery(index)"
      >
        <img :src="imageUrl" alt="Post image" class="post-image" />
        <div v-if="shouldShowOverlay(index)" class="image-overlay">+{{ hiddenImagesCount }}</div>
      </div>
    </div>

    <div class="post-actions">
      <button @click="toggleLike" class="like-button" :disabled="likeLoading">
        <span v-if="isLiked">❤️</span>
        <span v-else>🤍</span>
        {{ likesCount }}
        <span v-if="likeLoading" class="loading-spinner"></span>
      </button>
      <button @click="toggleComments" class="comments-toggle-button">
        <span v-if="showComments">Hide Comments</span>
        <span v-else-if="hasComments">View {{ post.comments.length }} Comments</span>
        <span v-else>Add a comment</span>
      </button>
    </div>

    <!-- Add comment input above comments list -->
    <div v-if="showComments" class="add-comment-section">
      <input
        type="text"
        v-model="newCommentContent"
        placeholder="Add a comment..."
        class="comment-input"
      />
      <button
        @click="submitComment"
        :disabled="commentLoading || !newCommentContent"
        class="comment-submit-button"
      >
        <span v-if="commentLoading">Posting...</span>
        <span v-else>Post</span>
      </button>
    </div>
    <div class="comments-section" v-if="showComments && hasComments">
      <div v-for="comment in post.comments" :key="comment.id" class="comment">
        <img
          v-if="comment.author?.profilePictureUrl"
          :src="comment.author.profilePictureUrl"
          :alt="`${comment.author.username} profile picture`"
          class="comment-author-avatar"
        />
        <div class="comment-content-wrapper">
          <span class="comment-author" @click="navigateTo(comment.author.username)"
            >{{ comment.author?.username || 'Unknown' }}:</span
          >
          <p class="comment-text">{{ comment.content }}</p>
          <time :datetime="comment.createdAt" class="comment-date">{{
            formatCommentDate(comment.createdAt)
          }}</time>
        </div>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { PostForCard, RawCommentFromApi, Comment, Account } from '../types'
import ImageGallery from './ImageGallery.vue'
import { likePost, unlikePost, deletePostById, createComment } from '../services/postService'
import { useAccountStore } from '../stores/accountStore'
import { useRouter } from 'vue-router'

const route = useRouter()
const navigateTo = (userName: String) => {
  route.push(`${userName}`)
}
// Click-outside directive
const vClickOutside = {
  beforeMount(el, binding) {
    el.clickOutsideEvent = function (event) {
      if (!(el === event.target || el.contains(event.target))) {
        binding.value()
      }
    }
    document.body.addEventListener('click', el.clickOutsideEvent)
  },
  unmounted(el) {
    document.body.removeEventListener('click', el.clickOutsideEvent)
  },
}

interface Props {
  post: PostForCard
}

const emit = defineEmits(['like-toggled', 'post-deleted', 'comment-added'])

const newCommentContent = ref('')
const commentLoading = ref(false)

const submitComment = async (): Promise<void> => {
  if (!newCommentContent.value) return
  commentLoading.value = true
  console.log('Attempting to submit comment:', newCommentContent.value);
  try {
    const response = await createComment(props.post.id, newCommentContent.value)
    console.log('Create comment API response:', response);
    // Check if the API returned data. If not, construct a temporary comment.
    let newComment: Comment;

    if (response.data) {
      const rawNewComment: RawCommentFromApi = response.data;
      console.log('Raw new comment from API: ', rawNewComment);

      newComment = {
        id: rawNewComment.id.value,
        content: rawNewComment.content,
        author: {
          id: rawNewComment.author?.id?.value || '',
          username: rawNewComment.author?.userName || 'Unknown',
          email: rawNewComment.author?.email || '',
          profilePictureUrl: rawNewComment.author?.pictureUrl || null,
        },
        createdAt: rawNewComment.createdAt,
      };
    } else {
      // API returned successful response (HTTP 200) but no data. Create a temporary local comment.
      console.warn('API returned successful response but no comment data. Creating a temporary local comment.');
      newComment = {
        id: `temp-${Date.now()}`, // Temporary ID
        content: newCommentContent.value,
        author: {
          id: accountStore.currentAccount?.id || '',
          username: accountStore.currentAccount?.username || '', // Ensure it's empty string if no username for safety
          email: accountStore.currentAccount?.email || '',
          profilePictureUrl: accountStore.accountProfilePicture || null,
        },
        createdAt: new Date().toISOString(),
      };
    }

    if (!props.post.comments) {
      props.post.comments = [];
    }
    props.post.comments.unshift(newComment);
    console.log('New comment added to post.comments:', newComment);
    console.log('Current post.comments array:', props.post.comments);
    newCommentContent.value = ''
  } catch (error) {
    console.error('Error creating comment:', error)
    // Log the error response if available
    if (error.response) {
      console.error('Error response data:', error.response.data);
      console.error('Error response status:', error.response.status);
      console.error('Error response headers:', error.response.headers);
    }
  } finally {
    commentLoading.value = false
  }
}

const props = defineProps<Props>()

const showComments = ref(false)

const accountStore = useAccountStore()
const likeLoading = ref(false)

const currentUserId = computed(() => accountStore.currentAccount?.id || '')

const isLiked = computed(() => {
  return props.post.likedBy?.includes(currentUserId.value) || false
})

const isPostOwner = computed(() => {
  return accountStore.currentAccount?.id === props.post.author?.id
})

const likesCount = computed((): number => {
  return props.post.likedBy?.length || 0
})

const formattedDate = computed((): string => {
  return new Date(props.post.createdAt).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
})

const hasImages = computed((): boolean => {
  return props.post.images && props.post.images.length > 0
})

const hasComments = computed((): boolean => {
  return props.post.comments && props.post.comments.length > 0
})

const displayedImages = computed(() => {
  return props.post.images.slice(0, 4)
})

const hiddenImagesCount = computed(() => {
  return Math.max(props.post.images.length - 4, 0)
})

const shouldShowOverlay = (index: number) => {
  return props.post.images.length > 4 && index === 3
}

const imageGridClass = computed(() => {
  const count = displayedImages.value.length
  if (count === 1) return 'single-image'
  if (count === 2) return 'two-images'
  if (count === 3) return 'three-images'
  return 'four-plus-images'
})

const galleryOpen = ref(false)
const galleryIndex = ref(0)

function openGallery(index: number) {
  galleryIndex.value = index
  galleryOpen.value = true
}

const toggleComments = (): void => {
  showComments.value = !showComments.value
}

const formatCommentDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

const toggleLike = async () => {
  likeLoading.value = true
  try {
    if (isLiked.value) {
      await unlikePost(props.post.id)
      // Remove current user from likedBy
      props.post.likedBy = props.post.likedBy.filter((id) => id !== currentUserId.value)
    } else {
      await likePost(props.post.id)
      // Add current user to likedBy
      if (!props.post.likedBy) props.post.likedBy = []
      props.post.likedBy.push(currentUserId.value)
    }
    emit('like-toggled', props.post.id, !isLiked.value)
  } catch (error) {
    console.error('Error toggling like:', error)
  } finally {
    likeLoading.value = false
  }
}

const showMenu = ref(false)
const showDeleteDialog = ref(false)

const closeMenu = () => {
  showMenu.value = false
}

const confirmDelete = () => {
  showDeleteDialog.value = true
  showMenu.value = false
  console.log('Delete confirmation dialog shown')
}

const deletePost = async () => {
  console.log('delete post exectured')
  try {
    const response = await deletePostById(props.post.id)
    console.log(response.status)
    if (response.status === 200) {
      emit('post-deleted', props.post.id)
    } else {
      console.error('Failed to delete post. Server responded with:', response.status)
      // Show error to user (could be implemented with a toast or alert)
    }
  } catch (error) {
    console.error('Error deleting post:', error)
  } finally {
    showDeleteDialog.value = false
  }
}
</script>

<style scoped>
.post-card {
  background-color: #fff;
  border: 1px solid #ddd;
  border-radius: 8px;
  margin-bottom: 20px;
  padding: 15px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  max-width: 600px;
  margin-left: auto;
  margin-right: auto;
}

.post-header {
  display: flex;
  align-items: center;
  margin-bottom: 10px;
}

.author-avatar,
.comment-author-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  object-fit: cover;
  margin-right: 10px;
  border: 1px solid #eee;
}

.author-info {
  display: flex;
  flex-direction: column;
}

.author-username {
  font-weight: bold;
  color: #333;
}

.post-header time {
  font-size: 0.85em;
  color: #777;
}

.post-content {
  margin-bottom: 15px;
  line-height: 1.6;
  color: #333;
}

.post-images {
  display: grid;
  gap: 2px;
  margin-bottom: 15px;
}

.single-image {
  display: block;
  max-height: 400px;
}

.single-image .image-container {
  display: flex;
  justify-content: center;
  align-items: center;
  aspect-ratio: auto;
  max-height: 400px;
}

.single-image .post-image {
  max-height: 100%;
  max-width: 600px;
  width: auto;
  object-fit: contain;
}

.two-images {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4px;
}

.three-images {
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-template-rows: auto;
  gap: 4px;
}

.three-images .image-container:first-child {
  grid-column: span 2;
}

.four-plus-images {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  grid-auto-rows: 1fr;
  gap: 4px;
}

.image-container {
  position: relative;
  overflow: hidden;
  cursor: pointer;
  aspect-ratio: 1/1; /* Square containers */
}

.post-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  object-position: center;
  display: block;
  transition: transform 0.3s ease;
}

.image-container:hover .post-image {
  transform: scale(1.03);
}

.image-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 24px;
  font-weight: bold;
}

.post-actions {
  display: flex;
  align-items: center;
  margin-top: 10px;
  border-top: 1px solid #eee;
  padding-top: 10px;
}

.like-button {
  background: none;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 1rem;
  padding: 5px 10px;
  border-radius: 5px;
  transition: background-color 0.2s;
}

.like-button:disabled {
  cursor: not-allowed;
  opacity: 0.7;
}

.like-button:hover:not(:disabled) {
  background-color: #f0f2f5;
}

.loading-spinner {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(0, 0, 0, 0.1);
  border-radius: 50%;
  border-top-color: #555;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.comments-toggle-button {
  background-color: #f0f2f5;
  border: none;
  border-radius: 5px;
  padding: 8px 12px;
  cursor: pointer;
  font-size: 0.9em;
  color: #555;
  transition: background-color 0.2s ease;
}

.comments-toggle-button:hover {
  background-color: #e0e2e5;
}

.post-actions-menu {
  position: relative;
  margin-left: auto;
}

.menu-button {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
}

.add-comment-section {
  display: flex;
  gap: 10px;
  margin-top: 15px;
  padding-top: 15px;
  border-top: 1px solid #eee;
  align-items: center;
}

.comment-input {
  flex-grow: 1;
  padding: 10px 12px;
  border: 1px solid #ddd;
  border-radius: 20px;
  font-size: 0.95em;
  outline: none;
  transition: border-color 0.2s;
}

.comment-input:focus {
  border-color: #007bff;
}

.comment-submit-button {
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 20px;
  padding: 10px 15px;
  cursor: pointer;
  font-size: 0.9em;
  font-weight: bold;
  transition: background-color 0.2s;
}

.comment-submit-button:hover:not(:disabled) {
  background-color: #0056b3;
}

.comment-submit-button:disabled {
  background-color: #a0c9f1;
  cursor: not-allowed;
}

.comments-section {
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid #eee;
}

.comment {
  display: flex;
  margin-bottom: 15px;
  padding: 10px;
  background-color: #f9f9f9;
  border-radius: 10px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.comment-author-avatar {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  object-fit: cover;
  margin-right: 10px;
  flex-shrink: 0; /* Prevent shrinking */
}

.comment-content-wrapper {
  flex-grow: 1;
}

.comment-author {
  font-weight: bold;
  color: #333;
  margin-right: 5px;
  cursor: pointer;
}

.comment-author:hover {
  text-decoration: underline;
}

.comment-text {
  margin: 2px 0 5px;
  color: #444;
  line-height: 1.5;
  word-wrap: break-word; /* Ensure long words wrap */
}

.comment-date {
  font-size: 0.75em;
  color: #888;
}

/* Updated styles for consistency with overall design */
.comments-toggle-button {
  background-color: #e9e9e9; /* Lighter background for toggle button */
  color: #333; /* Darker text for better readability */
  font-weight: 600; /* Slightly bolder text */
  padding: 10px 15px; /* More padding */
  border-radius: 25px; /* More rounded corners */
  transition:
    background-color 0.3s ease,
    transform 0.1s ease;
}

.comments-toggle-button:hover {
  background-color: #dcdcdc; /* Darker on hover */
  transform: translateY(-1px); /* Slight lift effect */
}

.comments-toggle-button:active {
  transform: translateY(0);
}

.add-comment-section {
  display: flex;
  gap: 10px;
  margin-top: 15px;
  padding-top: 15px;
  border-top: 1px solid #eee;
  align-items: center;
}

.comment-input {
  flex-grow: 1;
  padding: 10px 12px;
  border: 1px solid #ddd;
  border-radius: 20px;
  font-size: 0.95em;
  outline: none;
  transition: border-color 0.2s;
}

.comment-input:focus {
  border-color: #007bff;
}

.comment-submit-button {
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 20px;
  padding: 10px 15px;
  cursor: pointer;
  font-size: 0.9em;
  font-weight: bold;
  transition: background-color 0.2s;
}

.comment-submit-button:hover:not(:disabled) {
  background-color: #0056b3;
}

.comment-submit-button:disabled {
  background-color: #a0c9f1;
  cursor: not-allowed;
}

.comments-section {
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid #eee;
}

.comment {
  display: flex;
  margin-bottom: 15px;
  padding: 10px;
  background-color: #f9f9f9;
  border-radius: 10px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.comment-author-avatar {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  object-fit: cover;
  margin-right: 10px;
  flex-shrink: 0; /* Prevent shrinking */
}

.comment-content-wrapper {
  flex-grow: 1;
}

.comment-author {
  font-weight: bold;
  color: #333;
  margin-right: 5px;
  cursor: pointer;
}

.comment-author:hover {
  text-decoration: underline;
}

.comment-text {
  margin: 2px 0 5px;
  color: #444;
  line-height: 1.5;
  word-wrap: break-word; /* Ensure long words wrap */
}

.comment-date {
  font-size: 0.75em;
  color: #888;
}

/* Updated styles for consistency with overall design */
.comments-toggle-button {
  background-color: #e9e9e9; /* Lighter background for toggle button */
  color: #333; /* Darker text for better readability */
  font-weight: 600; /* Slightly bolder text */
  padding: 10px 15px; /* More padding */
  border-radius: 25px; /* More rounded corners */
  transition:
    background-color 0.3s ease,
    transform 0.1s ease;
}

.comments-toggle-button:hover {
  background-color: #dcdcdc; /* Darker on hover */
  transform: translateY(-1px); /* Slight lift effect */
}

.comments-toggle-button:active {
  transform: translateY(0);
}

.menu-dropdown {
  position: absolute;
  right: 0;
  background: white;
  border: 1px solid #ddd;
  border-radius: 4px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  z-index: 100;
}

.menu-item,
.delete-button {
  display: block;
  width: 100%;
  padding: 8px 16px;
  text-align: left;
  background: none;
  border: none;
  cursor: pointer;
}

.delete-button {
  color: #ff0000;
}

.menu-item:hover,
.delete-button:hover {
  background-color: #f5f5f5;
}

.dialog-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.confirmation-dialog {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  min-width: 300px;
}

.dialog-buttons {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 15px;
}

.confirm-delete {
  background-color: #ff0000;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
}

.comments-section {
  margin-top: 15px;
  padding-top: 15px;
  border-top: 1px solid #eee;
}

.comment {
  display: flex;
  align-items: flex-start;
  margin-bottom: 10px;
  background-color: #f9f9f9;
  padding: 8px 12px;
  border-radius: 6px;
}

.comment-content-wrapper {
  flex-grow: 1;
}

.comment-author {
  font-weight: bold;
  margin-right: 5px;
  color: #333;
  display: inline-block;
}

.comment-text {
  display: inline;
  margin: 0;
  color: #444;
}

.comment-date {
  font-size: 0.75em;
  color: #888;
  display: block;
  margin-top: 2px;
}
</style>
