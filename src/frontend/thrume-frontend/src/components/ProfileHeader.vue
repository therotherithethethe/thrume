<template>
  <div class="profile-header">
    <div class="profile-picture" @click="handleAvatarClick">
      <img v-if="pictureUrl && !avatarLoading" :src="pictureUrl" class="circular-image">
      <div v-else-if="!avatarLoading" class="placeholder circular-image">
        <svg width="60" height="60" viewBox="0 0 24 24">
          <circle cx="12" cy="12" r="10" fill="#e0e0e0"/>
          <path d="M12 12m-4 0a4 4 0 1 0 8 0a4 4 0 1 0 -8 0" fill="#9e9e9e"/>
        </svg>
      </div>
      <div v-if="avatarLoading" class="loading-overlay">
        <div class="spinner"></div>
      </div>
    </div>
    <input
      ref="fileInput"
      type="file"
      accept="image/jpeg,image/png"
      style="display: none"
      @change="handleFileChange"
    >
    
    <div class="profile-info">
      <h1>{{ userName }}</h1>
      <div class="stats">
        <div class="stat">
          <span class="count">{{ postCount }}</span>
          <span class="label">posts</span>
        </div>
        <div class="stat clickable" @click="openFollowersMenu">
          <span class="count">{{ followers }}</span>
          <span class="label">followers</span>
        </div>
        <div class="stat clickable" @click="openFollowingMenu">
          <span class="count">{{ following }}</span>
          <span class="label">following</span>
        </div>
      </div>
    </div>
    
    <div class="action-buttons">
      <FollowButton
        :is-own-profile="isOwnProfile"
        :is-following="amIFollowing"
        :target-account-id="targetAccountId"
        @follow-status-changed="handleFollowStatusChanged"
      />
      <StartConversationButton
        v-if="!isOwnProfile && userName"
        :targetUserName="userName"
        :isFollowingUser="amIFollowing"
        :isUserFollowingMe="isUserFollowingMe"
      />
    </div>

    <FollowersMenu
      :isOpen="currentMenu !== 'none'"
      :type="currentMenu === 'followers' ? 'followers' : 'following'"
      :userName="userName || ''"
      @close="closeMenu"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { useAccountStore } from '../stores/accountStore';
import FollowButton from './FollowButton.vue';
import FollowersMenu from './FollowersMenu.vue';
import StartConversationButton from './StartConversationButton.vue';
import { useToast } from 'vue-toast-notification'
import 'vue-toast-notification/dist/theme-sugar.css';

const accountStore = useAccountStore();
const toast = useToast();
const fileInput = ref<HTMLInputElement | null>(null);
const avatarLoading = ref(false);
const currentMenu = ref<'none' | 'followers' | 'following'>('none');

const props = defineProps({
  userName: String,
  pictureUrl: {
    type: String,
    default: null
  },
  followers: Number,
  following: Number,
  postCount: Number,
  amIFollowing: Boolean,
  isUserFollowingMe: Boolean,
  targetAccountId: {
    type: String,
    required: true
  }
});

const emit = defineEmits(['update-followers', 'follow-status-changed', 'avatar-updated']);

const isOwnProfile = computed(() => {
  return accountStore.accountId === props.targetAccountId;
});

const handleFollowStatusChanged = (newStatus: boolean) => {
  const delta = newStatus ? 1 : -1;
  emit('update-followers', delta);
  emit('follow-status-changed', newStatus);
};

const handleAvatarClick = () => {
  if (!isOwnProfile.value) return;
  fileInput.value?.click();
};

const handleFileChange = (event: Event) => {
  const input = event.target as HTMLInputElement;
  if (input.files && input.files.length > 0) {
    uploadAvatar(input.files[0]);
    input.value = ''; // Reset input
  }
};

async function uploadAvatar(file: File) {
  // Validate file type
  if (!['image/jpeg', 'image/png'].includes(file.type)) {
    toast.error('Invalid file type. Please upload a JPG or PNG image.');
    return;
  }
  
  // Validate file size
  if (file.size > 5 * 1024 * 1024) {
    toast.error('File size exceeds 5MB limit.');
    return;
  }
  
  avatarLoading.value = true;
  
  try {
    await accountStore.updateProfilePicture(file);
    toast.success('Avatar updated successfully!');
    emit('avatar-updated');
  } catch (error) {
    console.error('Avatar upload failed:', error);
    toast.error('Failed to update avatar. Please try again.');
  } finally {
    avatarLoading.value = false;
  }
}

const openFollowersMenu = () => {
  currentMenu.value = 'followers';
};

const openFollowingMenu = () => {
  currentMenu.value = 'following';
};

const closeMenu = () => {
  currentMenu.value = 'none';
};
</script>

<style scoped>
.profile-header {
  display: flex;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #e0e0e0;
  margin-bottom: 20px;
}

.circular-image {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid #fff;
  box-shadow: 0 2px 6px rgba(0,0,0,0.1);
}

.placeholder {
  display: flex;
  justify-content: center;
  align-items: center;
  background: #f5f5f5;
}

.profile-info {
  margin-left: 30px;
}

h1 {
  margin: 0 0 10px 0;
  font-size: 28px;
}

.stats {
  display: flex;
  gap: 20px;
}

.stat {
  text-align: center;
}

.stat.clickable {
  cursor: pointer;
  transition: background-color 0.2s ease;
  padding: 4px 8px;
  border-radius: 8px;
}

.stat.clickable:hover {
  background-color: rgba(0, 0, 0, 0.05);
}

.count {
  display: block;
  font-weight: bold;
  font-size: 18px;
}

.label {
  font-size: 14px;
  color: #666;
}

.action-buttons {
  display: flex;
  gap: 12px;
  margin-left: auto;
  align-items: center;
}
</style>

<style scoped>
.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(255, 255, 255, 0.7);
  border-radius: 50%;
  display: flex;
  justify-content: center;
  align-items: center;
}

.spinner {
  width: 30px;
  height: 30px;
  border: 3px solid rgba(0, 0, 0, 0.1);
  border-radius: 50%;
  border-top-color: #3498db;
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.profile-picture {
  position: relative;
  cursor: pointer;
}
</style>