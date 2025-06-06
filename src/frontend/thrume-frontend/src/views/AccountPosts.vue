<!-- views/AccountPosts.vue -->
<template>
  <div class="account-page">
    <div v-if="profileLoading" class="loading-message">
      Loading profile for {{ username }}...
    </div>
    <div v-else-if="profileError" class="error-message">
      Error: {{ profileError }}
    </div>
    <ProfileHeader
      v-else-if="profileData"
      :user-name="profileData.userName"
      :picture-url="profileData.pictureUrl ?? undefined"
      :followers="profileData.followersCount"
      :following="profileData.followingCount"
      :post-count="profileData.postCount"
      :am-i-following="profileData.amIFollowing"
      :is-user-following-me="profileData.isUserFollowingMe ?? false"
      :target-account-id="profileData.accountId"
      @update-followers="handleFollowersUpdate"
      @follow-status-changed="handleFollowStatusChanged"
      @avatar-updated="handleAvatarUpdated"
    />

    <div v-if="loading" class="loading-message">
      Loading posts for {{ username }}...
    </div>
    <div v-else-if="error" class="error-message">
      Error: {{ error }}
    </div>
    <div v-else-if="processedPosts.length">
      <PostCard
        v-for="post in processedPosts"
        :key="post.id"
        :post="post"
        @post-deleted="handlePostDeleted"
        @comment-added="handleCommentAdded"
      />
    </div>
    <div v-else class="no-posts-message">
      No posts found for {{ username }}.
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import PostCard from '../components/PostCard.vue';
import ProfileHeader from '../components/ProfileHeader.vue';
import {
  PostForCard,
  RawPostFromApi,
  Account,
  Comment,
  RawCommentFromApi,
  RawAccount,
  RawImage,
  ProfileData
} from '../types';
import apiClient from '../axiosInstance';
import { useAccountStore } from '../stores/accountStore';

const route = useRoute();
const accountStore = useAccountStore();

const username = ref<string>('');
const rawPosts = ref<RawPostFromApi[]>([]);
const processedPosts = ref<PostForCard[]>([]);
const loading = ref<boolean>(true);
const error = ref<string | null>(null);

// Profile data state
const profileData = ref<ProfileData | null>(null);
const profileLoading = ref<boolean>(true);
const profileError = ref<string | null>(null);

const transformRawAuthorToAccount = (
  rawAuthorId: string,
  rawUserName?: string | null,
  rawPictureUrl?: string | null,
  embeddedRawAccount?: RawAccount | null
): Account => {
  if (embeddedRawAccount && embeddedRawAccount.userName) {
    return {
      id: embeddedRawAccount.id.value,
      username: embeddedRawAccount.userName,
      email: embeddedRawAccount.email || '',
      profilePictureUrl: embeddedRawAccount.pictureUrl || null,
    };
  }
  else if (rawUserName) {
    return {
      id: rawAuthorId,
      username: rawUserName,
      email: '',
      profilePictureUrl: rawPictureUrl || null,
    };
  }
  return {
    id: rawAuthorId,
    username: `Unknown User (${rawAuthorId.substring(0, 8)})`,
    email: '',
    profilePictureUrl: null,
  };
};

const processImages = (rawImages: RawImage[]): string[] => {
  return rawImages.map(img => img?.id?.imageUrl).filter(url => typeof url === 'string') as string[];
};

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

const checkIfUserFollowsMe = async (targetUserName: string): Promise<boolean> => {
  // Only check if we have a current user
  if (!accountStore.accountUsername) {
    return false;
  }
  
  try {
    // Get the followers of the current user to see if the target user is among them
    const response = await apiClient.get(`/account/followers/${accountStore.accountUsername}`);
    const followers = response.data;
    
    // Check if the target user is in the followers list
    return followers.some((follower: any) => follower.userName === targetUserName);
  } catch (error) {
    console.error('Failed to check if user follows me:', error);
    return false;
  }
};

const fetchProfileData = async (name: string) => {
  profileLoading.value = true;
  profileError.value = null;
  
  try {
    const response = await apiClient.get<ProfileData>(`/account/profile/${name}`);
    const baseProfileData = response.data;
    
    // Check if the target user is following the current user
    const isUserFollowingMe = await checkIfUserFollowsMe(name);
    
    // Combine the profile data with the follow relationship info
    profileData.value = {
      ...baseProfileData,
      isUserFollowingMe
    };
  } catch (err: any) {
    console.error('Failed to fetch profile data:', err);
    profileError.value = 'Failed to load profile data.';
  } finally {
    profileLoading.value = false;
  }
};

const fetchPostsByUsername = async (name: string) => {
  loading.value = true;
  error.value = null;
  rawPosts.value = [];
  processedPosts.value = [];

  try {
    const response = await apiClient.get<RawPostFromApi[]>(`posts/${name}`);

    if (response.data && response.data.length > 0) {
      rawPosts.value = response.data;
      processedPosts.value = rawPosts.value.map(transformRawPostToFullPost);
    } else {
      error.value = `User "${name}" not found or has no posts.`;
    }
  } catch (err: any) {
    console.error('Failed to fetch posts:', err);
    if (err.response && err.response.status === 404) {
      error.value = `User "${name}" not found or has no posts.`;
    } else {
      error.value = 'Failed to load posts. Please try again.';
    }
  } finally {
    loading.value = false;
  }
};

const fetchAccountData = async (name: string) => {
  await fetchProfileData(name);
  await fetchPostsByUsername(name);
};

onMounted(() => {
  const currentUsernameParam = route.params.name;

  if (typeof currentUsernameParam === 'string') {
    username.value = currentUsernameParam;
    fetchAccountData(currentUsernameParam);
  } else if (Array.isArray(currentUsernameParam) && currentUsernameParam.length > 0) {
    username.value = currentUsernameParam[0];
    fetchAccountData(currentUsernameParam[0]);
  } else {
    error.value = "No username provided in the URL.";
    loading.value = false;
    profileLoading.value = false;
  }
});

watch(
  () => route.params.name,
  async (newUsernameParam) => {
    if (typeof newUsernameParam === 'string' && newUsernameParam !== username.value) {
      username.value = newUsernameParam;
      await fetchAccountData(newUsernameParam);
    }
  },
);

const handleCommentAdded = async (postId: string) => {
  await fetchPostsByUsername(username.value);
};

const handlePostDeleted = (postId: string) => {
  processedPosts.value = processedPosts.value.filter(post => post.id !== postId);
};

const handleFollowersUpdate = (delta: number) => {
  if (profileData.value) {
    profileData.value.followersCount += delta;
  }
};

const handleFollowStatusChanged = (newStatus: boolean) => {
  if (profileData.value) {
    profileData.value.amIFollowing = newStatus;
  }
};

const handleAvatarUpdated = () => {
  if (username.value) {
    fetchProfileData(username.value);
  }
};
</script>

<style scoped>
.account-page {
  padding: 20px;
  max-width: 800px;
  margin: 0 auto;
  text-align: center;
}

h2 {
  color: #2c3e50;
  margin-bottom: 25px;
}

.loading-message,
.error-message,
.no-posts-message {
  padding: 20px;
  background-color: #f0f2f5;
  border-radius: 8px;
  color: #555;
  margin-top: 20px;
}

.error-message {
  background-color: #ffe0e0;
  color: #d32f2f;
  border: 1px solid #d32f2f;
}

.post-card + .post-card {
  margin-top: 20px;
}
</style>