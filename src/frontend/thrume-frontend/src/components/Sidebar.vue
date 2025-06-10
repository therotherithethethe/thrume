<!-- components/TheSidebar.vue -->
<script setup lang="ts">
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/authStore';
import { useAccountStore } from '../stores/accountStore';
import { computed, ref } from 'vue';
import apiClient from '../axiosInstance';
import CreatePostForm from './CreatePostForm.vue';

// Import the icons we'll be using
import {
  Home,
  User,
  MessageSquare,
  PlusSquare,
  Search,
  Shield,
  LogIn,
  LogOut,
  UserPlus,
  Voicemail
} from 'lucide-vue-next';

const router = useRouter();
const authStore = useAuthStore();
const accountStore = useAccountStore();
const showCreatePostModal = ref(false);

// Computed properties to reactively get user info and login status
const currentUsername = computed(() => accountStore.accountUsername);
const isLoggedIn = computed(() => authStore.currentUser.isAuthenticated);
const isAdmin = computed(() => accountStore.roles.includes('Admin'));

const openCreatePostModal = () => {
  showCreatePostModal.value = true;
};

const logout = async () => {
  try {
    await apiClient.post('/api/auth/logout');
    authStore.currentUser.isAuthenticated = false;
    accountStore.clearAccount();
    router.push({ name: 'Auth' });
  } catch (error) {
    console.error('Error during logout:', error);
    alert('Logout failed. Please try again.');
  }
};
</script>

<template>
  <aside class="sidebar">
    <div class="sidebar-content">
      <div class="sidebar-header">
        <h3>Thrume</h3>
      </div>

      <!-- Main Navigation -->
      <nav class="sidebar-nav">
        <!-- Using <router-link> is the standard Vue way for navigation -->
        <router-link :to="{ name: 'Home' }" class="nav-item">
          <Home :size="20" />
          <span>Home</span>
        </router-link>

        <router-link v-if="isLoggedIn" :to="`/${currentUsername}`" class="nav-item">
          <User :size="20" />
          <span>Profile</span>
        </router-link>

        <router-link v-if="isLoggedIn" :to="{ name: 'Messages' }" class="nav-item">
          <MessageSquare :size="20" />
          <span>Messages</span>
        </router-link>

        <router-link v-if="isLoggedIn" :to="'/search/ '" class="nav-item">
          <Search :size="20" />
          <span>Search</span>
        </router-link>

        <button v-if="isLoggedIn" @click="openCreatePostModal" class="nav-item logout-btn">
          <PlusSquare :size="20" />
          <span>Create Post</span>
        </button>

        <router-link v-if="isAdmin" :to="'/admin/panel'" class="nav-item">
          <Shield :size="20" />
          <span>Admin</span>
        </router-link>

        <router-link v-if="isLoggedIn" :to="'/voice/call'" class="nav-item">
          <Voicemail :size="20" />
          <span>Voice</span>
        </router-link>

        <!-- Links for logged-out users -->
        <router-link v-if="!isLoggedIn" :to="{ name: 'Auth' }" class="nav-item">
          <LogIn :size="20" />
          <span>Login</span>
        </router-link>
        <router-link v-if="!isLoggedIn" :to="{ name: 'Register' }" class="nav-item">
          <UserPlus :size="20" />
          <span>Register</span>
        </router-link>
        
        <button v-if="isLoggedIn" @click="openCreatePostModal" class="nav-item logout-btn">
          <PlusSquare :size="20" />
          <span>Create Post</span>
        </button>
        <button v-if="isLoggedIn" @click="logout" class="nav-item logout-btn">
          <LogOut :size="20" />
          <span>Logout</span>
        </button>
      </nav>
    </div>

    <!-- The Create Post Modal remains the same -->
    <CreatePostForm v-if="showCreatePostModal" @close="showCreatePostModal = false" />
  </aside>
</template>

<style scoped>
/* Using CSS Variables for easier theme management */
:root {
  --sidebar-bg-color: #1a1d24;
  --sidebar-text-color: #adb5bd;
  --sidebar-accent-color: #3498db;
  --sidebar-hover-bg-color: #2c3e50;
  --sidebar-active-bg-color: #34495e;
  --sidebar-active-text-color: #ffffff;
  --brand-color: #3498db;
  --logout-bg-color: #e74c3c;
  --logout-hover-bg-color: #c0392b;
}

.sidebar {
  width: 250px; /* A bit wider for icons and text */
  background-color: var(--sidebar-bg-color);
  color: var(--sidebar-text-color);
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  padding: 1.5rem 1rem;
  box-shadow: 4px 0 15px rgba(0, 0, 0, 0.2);
}

.sidebar-content {
  display: flex;
  flex-direction: column;
  height: 100%; /* Make this container take full height */
}

.sidebar-header {
  margin-bottom: 2rem;
  padding-left: 0.75rem;
}

.sidebar-header h3 {
  margin: 0;
  font-size: 1.8em;
  font-weight: 700;
  color: var(--brand-color);
  cursor: default; /* It's a brand, not a link */
}

.sidebar-nav {
  display: flex;
  flex-direction: column;
  gap: 0.5rem; /* Space between nav items */
}

/* This is the new unified style for all navigation items (<router-link> and <button>) */
.nav-item {
  display: flex;
  align-items: center;
  gap: 1rem; /* Space between icon and text */
  padding: 0.75rem 1rem;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 500;
  text-decoration: none; /* Removes underline from links */
  color: var(--sidebar-text-color);
  background-color: transparent;
  border: none;
  width: 100%;
  cursor: pointer;
  transition: background-color 0.2s ease, color 0.2s ease;
}

.nav-item:hover {
  background-color: var(--sidebar-hover-bg-color);
  color: var(--sidebar-active-text-color);
}

/* This is the key for showing the active page! */
.nav-item.router-link-exact-active {
  background-color: var(--sidebar-active-bg-color);
  color: var(--sidebar-active-text-color);
  font-weight: 600;
}

/* This pushes the actions to the bottom */
.sidebar-actions {
  margin-top: auto;
  display: flex;
  flex-direction: column;
  gap: 0.5rem; /* Space between buttons */
  padding-top: 1rem; /* Add some space from the nav above */
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.create-post-btn {
  background-color: var(--sidebar-accent-color);
  color: white;
}
.create-post-btn:hover {
  background-color: #2980b9; /* Darker blue */
}

.logout-btn {
  background-color: transparent;
  color: var(--logout-bg-color);
  border: 1px solid var(--logout-bg-color);
}
.logout-btn:hover {
  background-color: var(--logout-bg-color);
  color: white;
}

</style>