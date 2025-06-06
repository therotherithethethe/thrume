<script setup lang="ts">
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

function handleLogout() {
  authStore.logout();
  router.push('/auth'); // Redirect to login after logout
}
</script>

<template>
  <div class="home-container">
    <h1>Welcome!</h1>
    <p v-if="authStore.isAuthenticated">You are logged in.</p>
    <p v-else>You are not logged in.</p>
    <button v-if="authStore.isAuthenticated" @click="handleLogout">Logout</button>
    <router-link v-else to="/auth">Go to Login</router-link>
  </div>
</template>

<style scoped>
.home-container {
  padding: 20px;
  text-align: center;
}
button {
  margin-top: 20px;
  padding: 10px 20px;
  cursor: pointer;
}
</style>