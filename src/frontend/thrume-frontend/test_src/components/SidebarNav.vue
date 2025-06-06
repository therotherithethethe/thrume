<template>
  <nav class="sidebar-nav">
    <div class="user-profile-summary" v-if="authStore.isAuthenticated && currentUser">
      <!-- Placeholder for avatar -->
      <div class="avatar-placeholder">
        <img v-if="currentUser.avatarUrl" :src="currentUser.avatarUrl" alt="User Avatar" class="avatar-image" />
        <span v-else class="initials">{{ getUserInitials(currentUser.username) }}</span>
      </div>
      <span class="username">{{ currentUser.username }}</span>
    </div>
    <ul>
      <li><router-link to="/">Home</router-link></li>
      <li v-if="authStore.isAuthenticated && currentUser">
        <router-link :to="`/${currentUser.username}`">My Profile</router-link>
      </li>
      <li><router-link to="/explore">Explore</router-link></li>
      <li v-if="authStore.isAuthenticated">
        <button @click="openCreatePostModal" class="create-post-btn">Create Post</button>
      </li>
    </ul>
    <button v-if="authStore.isAuthenticated" @click="handleLogout" class="logout-btn">Logout</button>
  </nav>
</template>

<script setup lang="ts">
import { computed, defineEmits } from 'vue'; // Added defineEmits
import { useAuthStore } from '@/stores/auth';
import { type CurrentUser } from '@/types/auth'; // Import CurrentUser from shared types
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();
const emit = defineEmits(['open-create-post']);

// Directly use the currentUser from the store.
const currentUser = computed(() => authStore.currentUser);

const getUserInitials = (name: string | undefined) => {
  if (!name) return '';
  return name.substring(0, 2).toUpperCase();
};

function openCreatePostModal() {
  emit('open-create-post'); // Emit the event
}

async function handleLogout() {
  await authStore.logout();
  router.push('/auth');
}
</script>

<style scoped>
.sidebar-nav {
  width: 250px;
  background-color: #f4f4f4;
  padding: 20px;
  height: 100vh;
  display: flex;
  flex-direction: column;
  border-right: 1px solid #ddd;
}

.user-profile-summary {
  display: flex;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 15px;
  border-bottom: 1px solid #eee;
}

.avatar-placeholder {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  background-color: #007bff;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2em;
  margin-right: 10px;
  overflow: hidden; /* To contain the image */
}

.avatar-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.username {
  font-weight: bold;
}

ul {
  list-style: none;
  padding: 0;
  margin: 0;
  flex-grow: 1;
}

li {
  margin-bottom: 10px;
}

a, .create-post-btn, .logout-btn {
  text-decoration: none;
  color: #333;
  padding: 8px 12px;
  display: block;
  border-radius: 4px;
  transition: background-color 0.2s ease-in-out;
}

a:hover, .create-post-btn:hover, .logout-btn:hover {
  background-color: #e9e9e9;
}

a.router-link-active {
  background-color: #007bff;
  color: white;
}

.create-post-btn, .logout-btn {
  background-color: #007bff;
  color: white;
  border: none;
  cursor: pointer;
  text-align: center;
  width: 100%;
  margin-top: 10px;
}

.logout-btn {
  background-color: #dc3545;
  margin-top: auto; /* Pushes logout button to the bottom */
}
</style>