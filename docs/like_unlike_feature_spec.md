# Like/Unlike Feature Specification

## Overview
Implement like/unlike functionality for posts with:
- Like button in PostCard component
- API calls to /posts/like and /posts/unlike endpoints
- Real-time UI updates
- Error handling

## API Service Methods

### postService.ts
```typescript:src/frontend/thrume-frontend/src/services/postService.ts
import axiosInstance from '../axiosInstance';

export const likePost = (postId: string) => {
  return axiosInstance.post('/posts/like', { value: postId });
};

export const unlikePost = (postId: string) => {
  return axiosInstance.post('/posts/unlike', { value: postId });
};
```

## Component Changes

### PostCard.vue Template
```html:src/frontend/thrume-frontend/src/components/PostCard.vue
<!-- Add to post-actions section -->
<button @click="toggleLike" class="like-button" :disabled="likeLoading">
  <span v-if="isLiked">❤️</span>
  <span v-else>🤍</span>
  {{ likesCount }}
  <span v-if="likeLoading" class="loading-spinner"></span>
</button>
```

### PostCard.vue Script
```typescript:src/frontend/thrume-frontend/src/components/PostCard.vue
import { likePost, unlikePost } from '../services/postService';
import { useAccountStore } from '../stores/accountStore';

// Add inside setup()
const accountStore = useAccountStore();
const likeLoading = ref(false);

const currentUserId = computed(() => accountStore.account?.id || '');

const isLiked = computed(() => {
  return props.post.likedBy?.includes(currentUserId.value) || false;
});

const likesCount = computed(() => {
  return props.post.likedBy?.length || 0;
});

const toggleLike = async () => {
  likeLoading.value = true;
  try {
    if (isLiked.value) {
      await unlikePost(props.post.id);
    } else {
      await likePost(props.post.id);
    }
    // Update local state optimistically
    if (isLiked.value) {
      props.post.likedBy = props.post.likedBy.filter(id => id !== currentUserId.value);
    } else {
      if (!props.post.likedBy) props.post.likedBy = [];
      props.post.likedBy.push(currentUserId.value);
    }
  } catch (error) {
    console.error('Error toggling like:', error);
    // Show error notification
  } finally {
    likeLoading.value = false;
  }
};
```

## Store Integration

### accountStore.ts
Ensure the store provides current user ID:
```typescript:src/frontend/thrume-frontend/src/stores/accountStore.ts
// Add to accountStore
account: null as Account | null, // Should contain id property
```

## Error Handling
- Disable button during API calls
- Show loading spinner
- Log errors to console
- (Future) Integrate with notification system

## Visual Design
- Filled heart (❤️) for liked state
- Outline heart (🤍) for unliked state
- Loading spinner during API calls
- Button disabled during loading

## Validation Checklist
- [ ] Like button toggles heart icon
- [ ] Like count updates correctly
- [ ] API calls are made to correct endpoints
- [ ] Current user ID is correctly retrieved
- [ ] Button disabled during loading
- [ ] Error handling works