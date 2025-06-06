<script setup lang="ts">
import { ref, onMounted } from 'vue'; // Import onMounted for initial data fetch
import apiClient from '../axiosInstance';
import PostCard from "../components/PostCard.vue";
// Import necessary types and transformation helpers
import { PostForCard, RawPostFromApi, Account, Comment, RawCommentFromApi, RawAccount, RawImage } from '../types';

// --- Reactive State for Posts Feed ---
const posts = ref<PostForCard[]>([]);
const loading = ref<boolean>(false);
const error = ref<string | null>(null);
const offset = ref<number>(0);
const limit = ref<number>(5); // Fetch 5 posts at a time
const hasMorePosts = ref<boolean>(true); // To control "Load More" button visibility

// --- Transformation Functions (Copied from AccountPosts.vue for self-containment) ---
// In a larger project, these might be moved to a shared `utils` file.

/**
 * Transforms raw author data (either from post's top-level or comment's embedded)
 * into an enriched Account object.
 * Provides a fallback if the author data is missing or null.
 */
const transformRawAuthorToAccount = (
  rawAuthorId: string,
  rawUserName?: string | null, // For post-level author
  rawPictureUrl?: string | null, // For post-level author
  embeddedRawAccount?: RawAccount | null // For comment-level embedded author
): Account => {
  // Prioritize embedded raw account if available and populated
  if (embeddedRawAccount && embeddedRawAccount.userName) {
    return {
      id: embeddedRawAccount.id.value,
      username: embeddedRawAccount.userName,
      email: embeddedRawAccount.email || '',
      profilePictureUrl: embeddedRawAccount.pictureUrl || null,
    };
  }
  // Otherwise, use the direct userName/pictureUrl if provided (for post-level)
  else if (rawUserName) {
    return {
      id: rawAuthorId,
      username: rawUserName,
      email: '', // Email is not provided at this level in your JSON
      profilePictureUrl: rawPictureUrl || null,
    };
  }
  // Fallback if no specific author data is available
  return {
    id: rawAuthorId,
    username: `Unknown User (${rawAuthorId.substring(0, 8)})`,
    email: '',
    profilePictureUrl: null,
  };
};

/**
 * Processes raw image data to extract the URL.
 * It maps an array of RawImage objects to an array of string URLs.
 */
const processImages = (rawImages: RawImage[]): string[] => {
  return rawImages.map(img => img?.id?.imageUrl).filter(url => typeof url === 'string') as string[];
};

/**
 * Transforms a raw comment object into an enriched Comment object.
 * Uses the embedded author if available, otherwise a fallback.
 */
const transformRawCommentToComment = (rawComment: RawCommentFromApi): Comment => {
  const commentAuthor = transformRawAuthorToAccount(
    rawComment.authorId.value,
    null,
    null,
    rawComment.author
  );
  return {
    id: rawComment.id.value,
    content: rawComment.content,
    author: commentAuthor,
    createdAt: rawComment.createdAt,
  };
};

/**
 * Transforms a raw post object received from the API into an enriched format
 * suitable for the PostCard component.
 * This directly uses the `userName` and `pictureUrl` from the raw post,
 * and transforms embedded comments.
 */
const transformRawPostToFullPost = (rawPost: RawPostFromApi): PostForCard => {
  const postAuthor = transformRawAuthorToAccount(
    rawPost.authorId.value,
    rawPost.userName,
    rawPost.pictureUrl
  );

  const likedByGuids = rawPost.likedBy.map(item => item.value);
  const images = processImages(rawPost.images);
  const comments = rawPost.comments.map(transformRawCommentToComment);

  return {
    id: rawPost.id.value,
    content: rawPost.content,
    images: images,
    author: postAuthor,
    likedBy: likedByGuids,
    comments: comments,
    createdAt: rawPost.createdAt,
  };
};

// --- Fetching Logic for Recent Posts ---
const fetchRecentPosts = async () => {
  if (loading.value || !hasMorePosts.value) {
    return; // Prevent multiple fetches or fetching if no more posts
  }

  loading.value = true;
  error.value = null;

  try {
    const response = await apiClient.get<RawPostFromApi[]>('/account/posts/recent', {
      params: {
        offset: offset.value,
        limit: limit.value
      }
    });

    if (response.data && response.data.length > 0) {
      const newPosts = response.data.map(transformRawPostToFullPost);
      posts.value.push(...newPosts); // Append new posts to the existing array
      offset.value += newPosts.length; // Increment offset by the number of posts received
      hasMorePosts.value = newPosts.length === limit.value; // If we got less than limit, no more pages
    } else {
      hasMorePosts.value = false; // No data, so no more posts
      if (posts.value.length === 0) {
        error.value = "No recent posts found from your subscriptions.";
      }
    }
  } catch (err: any) {
    console.error('Failed to fetch recent posts:', err);
    error.value = 'Failed to load recent posts. Please try again.';
    hasMorePosts.value = false; // Assume no more posts on error
  } finally {
    loading.value = false;
  }
};

// --- Initial Data Fetch on Component Mount ---
onMounted(() => {
  fetchRecentPosts();
});
</script>

<template>
  <section class="posts-feed">
    <h2>Recent Posts</h2>

    <div v-if="loading && posts.length === 0" class="loading-message">
      Loading your feed...
    </div>
    <div v-else-if="error && posts.length === 0" class="error-message">
      Error: {{ error }}
    </div>
    <div v-else-if="posts.length">
      <PostCard v-for="post in posts" :key="post.id" :post="post" />

      <div v-if="hasMorePosts && !loading" class="load-more-container">
        <button @click="fetchRecentPosts" class="load-more-button">Load More Posts</button>
      </div>
      <div v-else-if="loading" class="loading-message">
        Loading more...
      </div>
      <div v-else class="no-more-posts-message">
        You've reached the end of your feed.
      </div>
    </div>
    <div v-else class="no-posts-message">
      No recent posts from your subscriptions. Follow more users!
    </div>
  </section>

  <footer class="page-footer">
    <p>© 2025 Thrume</p>
  </footer>
</template>

<style scoped>
/* Scoped styles specific to Home.vue content */
.posts-feed {
  max-width: 800px;
  margin: 0 auto;
  background-color: #fff;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  display: flex;
  flex-direction: column;
}

.posts-feed h2 {
  margin-bottom: 20px;
  color: #333;
  text-align: center;
}

.post-card + .post-card {
  margin-top: 15px; /* Spacing between posts */
}

.loading-message,
.error-message,
.no-posts-message,
.no-more-posts-message {
  padding: 15px;
  background-color: #f0f2f5;
  border-radius: 8px;
  color: #555;
  text-align: center;
  margin-top: 20px;
}

.error-message {
  background-color: #ffe0e0;
  color: #d32f2f;
  border: 1px solid #d32f2f;
}

.load-more-container {
  text-align: center;
  margin-top: 20px;
}

.load-more-button {
  background-color: #007bff;
  color: white;
  padding: 10px 20px;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 1em;
  transition: background-color 0.2s ease;
}

.load-more-button:hover {
  background-color: #0056b3;
}

.page-footer {
  text-align: center;
  padding: 15px 0;
  margin-top: 20px;
  font-size: 0.9em;
  color: #555;
  background-color: #f8f9fa;
}
</style>