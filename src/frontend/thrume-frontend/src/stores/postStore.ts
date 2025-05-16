import { defineStore } from 'pinia';
import { ref } from 'vue';
import { apiGetUserPosts, apiCreatePost, apiLikePost, type Post } from '@/services/postApi';
import { useAuthStore } from './auth'; // To check current user for likes

export const usePostStore = defineStore('posts', () => {
  // --- State ---
  // Store posts keyed by username for profile views
  const userPosts = ref<Record<string, Post[]>>({});
  // Store posts for the main feed (if applicable later)
  const feedPosts = ref<Post[]>([]);
  // Store posts for the explore page (if applicable later)
  const explorePosts = ref<Post[]>([]);
  // Loading and error states (can be more granular per action/view)
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  // --- Actions ---

  // Fetch posts for a specific user profile
  async function fetchUserPosts(username: string) {
    isLoading.value = true;
    error.value = null;
    try {
      const posts = await apiGetUserPosts(username);
      userPosts.value[username] = posts;
    } catch (err: any) {
      console.error(`Failed to fetch posts for ${username}:`, err);
      error.value = err.message || 'Failed to load posts.';
      userPosts.value[username] = []; // Clear or keep stale data? Clear for now.
    } finally {
      isLoading.value = false;
    }
  }

  // Create a new post
  async function createPost(formData: FormData) {
    // No loading state change here, handled in component for form feedback
    error.value = null; // Clear previous errors
    try {
      const newPostData = await apiCreatePost(formData);
      // TODO: Process the newPostData if backend returns it
      // For now, we assume success means we should refresh relevant feeds/profiles
      console.log('Post created via API (response data):', newPostData);

      // Refresh posts for the current user's profile if they are viewing it
      const authStore = useAuthStore();
      if (authStore.currentUser) {
        // Re-fetch posts for the current user to include the new one
        // This is simple but might not be the most efficient way
        await fetchUserPosts(authStore.currentUser.username);
      }
      // Optionally, update feedPosts or explorePosts if the new post should appear there immediately

    } catch (err: any) {
      console.error('Failed to create post:', err);
      error.value = err.message || 'Failed to create post.';
      throw err; // Re-throw error so component can catch it
    }
  }

  // Toggle like status for a post
  async function togglePostLike(postIdValue: string, sourceUsername?: string) {
    // sourceUsername helps update the correct list (userPosts[username])
    error.value = null;
    const authStore = useAuthStore();
    if (!authStore.currentUser) {
        console.error("Cannot like post: User not logged in.");
        return; // Or throw error
    }
    const currentUserId = authStore.currentUser.id;

    // Find the post in the relevant list(s) to update optimistically
    let postToUpdate: Post | undefined;
    let postList: Post[] | undefined;

    if (sourceUsername && userPosts.value[sourceUsername]) {
        postList = userPosts.value[sourceUsername];
        postToUpdate = postList?.find(p => p.id.value === postIdValue);
    }
    // TODO: Also check feedPosts and explorePosts if needed

    if (!postToUpdate) {
        console.error(`Post with ID ${postIdValue} not found in local store for source ${sourceUsername || 'feed/explore'}. Cannot toggle like.`);
        return; // Or fetch the post first?
    }

    // Optimistic UI update
    const originallyLiked = postToUpdate.likedBy.includes(currentUserId);
    if (originallyLiked) {
        postToUpdate.likedBy = postToUpdate.likedBy.filter(id => id !== currentUserId);
    } else {
        postToUpdate.likedBy.push(currentUserId);
    }

    try {
        // Call the API
        await apiLikePost(postIdValue);
        // API call successful, optimistic update is now confirmed
        console.log(`Like toggled successfully for post ${postIdValue}`);
    } catch (err: any) {
        console.error(`Failed to toggle like for post ${postIdValue}:`, err);
        error.value = err.message || 'Failed to update like status.';
        // Revert optimistic update on failure
        if (postToUpdate) {
            if (originallyLiked) {
                // It failed to unlike, so add user back
                 if (!postToUpdate.likedBy.includes(currentUserId)) {
                    postToUpdate.likedBy.push(currentUserId);
                 }
            } else {
                // It failed to like, so remove user
                postToUpdate.likedBy = postToUpdate.likedBy.filter(id => id !== currentUserId);
            }
        }
        // Optionally re-throw or handle error further
    }
  }

  // --- Placeholder Actions for Future Features ---
  async function fetchFeedPosts() {
    console.log("Placeholder: fetchFeedPosts");
    // Implementation using GET /feed endpoint
  }

  async function fetchExplorePosts() {
    console.log("Placeholder: fetchExplorePosts");
    // Implementation using GET /posts/all or similar endpoint
  }

  // --- Return state and actions ---
  return {
    userPosts,
    feedPosts,
    explorePosts,
    isLoading,
    error,
    fetchUserPosts,
    createPost,
    togglePostLike,
    fetchFeedPosts,
    fetchExplorePosts,
  };
});