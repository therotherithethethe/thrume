<template>
  <button :disabled="loading" @click="handleClick" :class="buttonClasses">
    <span v-if="loading">...</span>
    <span v-else>{{ buttonText }}</span>
  </button>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import apiClient from '../axiosInstance';

const props = defineProps({
  isOwnProfile: Boolean,
  isFollowing: Boolean,
  targetAccountId: {
    type: String,
    required: true
  }
});

const emit = defineEmits(['follow-status-changed']);

const loading = ref(false);
const error = ref<string | null>(null);

const buttonText = computed(() => {
  if (props.isOwnProfile) return 'Edit Profile';
  return props.isFollowing ? 'Unfollow' : 'Follow';
});

const buttonClasses = computed(() => ({
  'follow-button': true,
  'own-profile': props.isOwnProfile,
  'following': props.isFollowing && !props.isOwnProfile,
  'loading': loading.value
}));

const handleClick = async () => {
  console.log('FollowButton clicked');
  console.log('isOwnProfile:', props.isOwnProfile);
  console.log('isFollowing:', props.isFollowing);
  console.log('targetAccountId:', props.targetAccountId);
  
  if (props.isOwnProfile) {
    console.log('Edit profile clicked');
    return;
  }

  // Ensure targetAccountId is present
  if (!props.targetAccountId) {
    const errorMsg = 'Missing account ID in FollowButton';
    console.error(errorMsg);
    error.value = errorMsg;
    return;
  }

  loading.value = true;
  try {
    const endpoint = props.isFollowing
      ? `/api/account/unfollow/${props.targetAccountId}`
      : `/api/account/follow/${props.targetAccountId}`;
    
    console.log('Calling API endpoint:', endpoint);
    
    const response = await apiClient.post(endpoint);
    console.log('API response:', response);
    
    emit('follow-status-changed', !props.isFollowing);
  } catch (err) {
    const errorMsg = props.isFollowing
      ? 'Failed to unfollow'
      : 'Failed to follow';
    
    console.error(errorMsg, err);
    error.value = errorMsg;
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.follow-button {
  padding: 8px 16px;
  border-radius: 4px;
  font-weight: bold;
  cursor: pointer;
  transition: background-color 0.2s;
}

.follow-button.own-profile {
  background-color: #f0f0f0;
  color: #333;
}

.follow-button:not(.own-profile) {
  background-color: #3897f0;
  color: white;
}

.follow-button.following {
  background-color: #ff4b5c;
}

.follow-button.loading {
  opacity: 0.7;
  cursor: not-allowed;
}
</style>