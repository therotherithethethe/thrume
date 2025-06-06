<!-- App.vue -->
<script setup lang="ts">
import TheSidebar from './components/Sidebar.vue'; // Import the new sidebar component
import { onMounted } from 'vue';
import { useAuthStore } from './stores/authStore';
import { useAccountStore } from './stores/accountStore';
import { RawAccountFromApi } from './types'; // Import for typing
import apiClient from './axiosInstance'; // Assuming apiClient is defined

const authStore = useAuthStore();
const accountStore = useAccountStore();

// Helper to fetch and store current user's account data
async function fetchAndStoreCurrentAccount() {
  try {
    const accountResponse = await apiClient.get<RawAccountFromApi>('account/me');
    if (accountResponse.status === 200 && accountResponse.data) {
      accountStore.setAccount(accountResponse.data);
      console.log('Account data stored:', accountStore.currentAccount);
      return true;
    } else {
      console.warn('Failed to fetch account/me data:', accountResponse);
      return false;
    }
  } catch (err: any) {
    console.error('Error fetching account/me:', err);
    accountStore.clearAccount(); // Clear account if fetch fails (e.g., token invalid)
    return false;
  }
}

// On mount, check auth status. If authenticated, fetch user account data.
// This ensures the sidebar can display correct user info when the app loads.
onMounted(async () => {
  const isAuthenticated = await authStore.checkAuth();
  if (isAuthenticated) {
    await fetchAndStoreCurrentAccount();
  } else {
    accountStore.clearAccount(); // Ensure account is cleared if not authenticated
  }
});
</script>

<template>
  <div id="app-layout">
    <TheSidebar />
    <main class="main-content">
      <router-view /> <!-- This is where your route components (Home, AccountPosts, Auth) will render -->
    </main>
  </div>
</template>

<style>
/* Global styles for the app layout */
#app-layout {
  display: flex; /* Use flexbox for sidebar and main content */
  min-height: 100vh; /* Ensure it takes full viewport height */
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
  box-sizing: border-box; /* Include padding and border in the element's total width and height */
}

.main-content {
  flex-grow: 1; /* Main content takes remaining space */
  padding: 20px;
  background-color: #f8f9fa; /* Light background for content area */
  overflow-y: auto; /* Allow scrolling for content if it overflows */
  display: flex; /* Helps center content vertically if needed, or align its children */
  flex-direction: column;
}

/* Optional: Resetting some default browser styles */
body, h1, h2, h3, p, ul, ol, li, figure, figcaption, blockquote, dl, dd {
  margin: 0;
  padding: 0;
}

body {
  line-height: 1.6;
  background-color: #f8f9fa; /* Match main-content background */
}

a {
  text-decoration: none;
  color: inherit;
}
</style>