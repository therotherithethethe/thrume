<!-- components/TheSidebar.vue -->
<script setup lang="ts">
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/authStore';
import { useAccountStore } from '../stores/accountStore';
import { computed, ref } from 'vue';
import apiClient from '../axiosInstance';
import CreatePostForm from './CreatePostForm.vue';

const router = useRouter();
const authStore = useAuthStore();
const accountStore = useAccountStore();
const showCreatePostModal = ref(false);

// Computed properties to reactively get user info and login status
const currentUsername = computed(() => accountStore.accountUsername);
const isLoggedIn = computed(() => authStore.currentUser.isAuthenticated);

// Navigation functions
const navigateToHome = () => {
  router.push({ name: 'Home' });
};

const navigateToProfile = () => {
  if (isLoggedIn.value && currentUsername.value !== 'Guest') {
    router.push({ path: `/${currentUsername.value}` }); // Use the username from the store
  } else {
    alert('Please log in to view your profile.');
    router.push({ name: 'Auth' });
  }
};

const navigateToMessages = () => {
  if (isLoggedIn.value) {
    router.push({ name: 'Messages' });
  } else {
    alert('Please log in to view messages.');
    router.push({ name: 'Auth' });
  }
};

const openCreatePostModal = () => {
  showCreatePostModal.value = true;
};

const logout = async () => {
  try {
    await apiClient.post('/auth/logout');
    authStore.currentUser.isAuthenticated = false; // Update auth status
    accountStore.clearAccount(); // Clear account data on logout
    router.push({ name: 'Auth' }); // Redirect to auth page
  } catch (error) {
    console.error('Error during logout:', error);
    alert('Logout failed. Please try again.');
  }
};
</script>

<template>
  <aside class="sidebar">
    <div class="sidebar-header">
      <h3>Thrume</h3>
    </div>
    <nav class="sidebar-nav">
      <ul>
        <li><p @click="navigateToHome" class="nav-link">Home</p></li>
        <!-- Only show profile/messages/logout if logged in -->
        <li v-if="isLoggedIn"><p @click="navigateToProfile" class="nav-link">Profile</p></li>
        <li v-if="isLoggedIn"><p @click="navigateToMessages" class="nav-link">Messages</p></li>
        <li v-if="isLoggedIn"><p @click="openCreatePostModal" class="nav-link">Create Post</p></li>
        <li v-if="isLoggedIn"><input type="button" value="Search" @click="router.push('/search/ ')"></li>

        <li v-if="isLoggedIn"><input type="button" value="Logout" @click="logout"></li>
        <!-- Show login button if not logged in -->
        <li v-if="!isLoggedIn"><p @click="router.push({name: 'Auth'})" class="nav-link">Login</p></li>
        <li v-if="!isLoggedIn"><p @click="router.push({name: 'Register'})" class="nav-link">Register</p></li>
      </ul>
    </nav>
    <CreatePostForm v-if="showCreatePostModal" @close="showCreatePostModal = false" />
  </aside>
</template>

<style scoped>
.sidebar {
  width: 200px; /* Fixed width sidebar */
  background-color: #2c3e50; /* Darker background */
  color: #ecf0f1; /* Light text */
  padding: 20px;
  box-shadow: 2px 0 5px rgba(0,0,0,0.1);
  min-height: 100vh; /* Full height */
  display: flex;
  flex-direction: column;
  flex-shrink: 0; /* Prevent sidebar from shrinking */
}

.sidebar-header {
  margin-bottom: 20px;
  border-bottom: 1px solid rgba(255,255,255,0.1);
  padding-bottom: 10px;
}

.sidebar-header h3 {
  margin: 0;
  font-size: 1.5em;
  color: #3498db; /* A blue accent */
}

.sidebar-nav ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.sidebar-nav li {
  margin-bottom: 15px;
}

.nav-link,
.sidebar-nav input[type="button"] {
  display: block;
  width: 100%;
  padding: 10px 15px;
  border-radius: 5px;
  text-align: left;
  font-size: 1.1em;
  color: #ecf0f1;
  background-color: transparent;
  border: none;
  cursor: pointer;
  transition: background-color 0.3s ease, color 0.3s ease;
}

.nav-link:hover,
.sidebar-nav input[type="button"]:hover {
  background-color: #34495e; /* Slightly lighter dark */
  color: #ffffff;
}

.sidebar-nav input[type="button"] {
  background-color: #e74c3c; /* Red for logout */
  color: white;
  margin-top: 10px;
}

.sidebar-nav input[type="button"]:hover {
  background-color: #c0392b;
}
</style>