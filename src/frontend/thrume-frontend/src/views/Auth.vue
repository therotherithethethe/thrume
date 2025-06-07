<!-- views/Auth.vue -->
<script setup lang="ts">
import apiClient from "../axiosInstance"
import { onMounted, ref } from 'vue';
import { useAuthStore } from "../stores/authStore";
import { useAccountStore } from "../stores/accountStore";
import { useRouter } from "vue-router";
import { RawAccountFromApi } from "../types";

const email = ref('lexoradka280211@gmail.com');
const password = ref('123456');

const authStore = useAuthStore();
const accountStore = useAccountStore();
const router = useRouter();

async function fetchAndStoreCurrentAccount() {
  try {
    const accountResponse = await apiClient.get<RawAccountFromApi>('account/me');
    if (accountResponse.status === 200 && accountResponse.data) {
      accountStore.setAccount(accountResponse.data);
      
      return true;
    } else {
      console.warn('Failed to fetch account/me data:', accountResponse);
      return false;
    }
  } catch (err: any) {
    console.error('Error fetching account/me:', err);
    accountStore.clearAccount();
    return false;
  }
}

async function handleLogin() {
  console.log('Attempting login...');
  try {
    const loginResponse = await apiClient.post('auth/login?useCookies=true', {
      email: email.value,
      password: password.value,
      twoFactorCode: null,
      twoFactorRecoveryCode: null
    });

    if (loginResponse.status === 200) {
      console.log('Login successful!');
      const isAuthenticated = await authStore.checkAuth();
      if (isAuthenticated) {
        const accountFetched = await fetchAndStoreCurrentAccount();
        if (accountFetched) {
          router.push({ name: 'Home' });
        } else {
          console.error('Login successful but failed to load account data. User might not be fully logged in visually.');
          router.push({ name: 'Home' });
        }
      } else {
        console.error('Login successful but authStore.checkAuth() returned false.');
        alert('Login successful, but session could not be verified. Please try again.');
      }
    } else {
      console.warn('Login failed with status:', loginResponse.status);
      alert('Login failed. Please check your credentials.');
    }
  } catch (e: any) {
    console.error('Login request error:', e);
    if (e.response && e.response.data && e.response.data.message) {
      alert(`Login error: ${e.response.data.message}`);
    } else {
      alert('An unexpected error occurred during login. Please try again.');
    }
  }
}

// Redirect to home if already authenticated on component mount
onMounted(async () => {
  const isAuthenticated = await authStore.checkAuth();
  if (isAuthenticated) {
    await fetchAndStoreCurrentAccount();
    router.push({name: 'Home'});
  }
});
</script>

<template>
  <div class="form-container">
    <div>
      <label for="email">Email:</label>
      <input type="email" name="email" id="email" v-model="email">
    </div>

    <div>
      <label for="password">Пароль:</label>
      <input type="password" name="password" id="password" v-model="password">
    </div>
    <input type="button" value="Login" @click="handleLogin">
  </div>
</template>

<style scoped>
.form-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: calc(100vh - 40px); /* Adjust based on App.vue padding/margin */
  width: 100%;
}

div {
  margin-bottom: 10px;
}

label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
  color: #333;
}

input[type="email"],
input[type="password"] {
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 4px;
  width: 250px;
  font-size: 1em;
}

input[type="button"] {
  background-color: #007bff;
  color: white;
  padding: 10px 20px;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 1em;
  transition: background-color 0.2s ease;
}

input[type="button"]:hover {
  background-color: #0056b3;
}
</style>