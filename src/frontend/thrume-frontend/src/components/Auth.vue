<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

// --- State ---
const activeTab = ref<'login' | 'register'>('login');

// Login form fields
const loginEmail = ref('');
const loginPassword = ref('');

// Register form fields
const registerEmail = ref('');
const registerPassword = ref('');

// --- Methods ---
async function handleLogin() {
  if (!loginEmail.value || !loginPassword.value) {
    authStore.error = 'Please enter both email and password.'; // Basic client-side validation
    return;
  }
  const success = await authStore.login({
    email: loginEmail.value,
    password: loginPassword.value,
  });
  if (success) {
    router.push('/'); // Redirect to home on successful login
  }
}

async function handleRegister() {
  if (!registerEmail.value || !registerPassword.value) {
     authStore.error = 'Please enter both email and password.'; // Basic client-side validation
     return;
  }
  // Optional: Add password confirmation field and check here
  const success = await authStore.register({
    email: registerEmail.value,
    password: registerPassword.value,
  });
  if (success) {
    // Switch to login tab and clear registration form
    activeTab.value = 'login';
    registerEmail.value = '';
    registerPassword.value = '';
    // Keep the success message visible (it's handled by the store state)
    // Error will be cleared by the watch effect below
  }
}

// --- Watchers ---
// Clear errors and reset registration success when tab changes
watch(activeTab, () => {
  authStore.clearError();
  // Only reset success flag if we are *not* on the login tab
  // This allows the success message to show after redirecting from register
  if (activeTab.value !== 'login') {
      authStore.resetRegistrationSuccess();
  }
});

// --- Lifecycle Hooks ---
// Clear errors when component mounts
onMounted(() => {
  authStore.clearError();
  // If registration was just successful, ensure we are on the login tab
  if (authStore.registrationSuccess) {
      activeTab.value = 'login';
  }
});

// Reset registration success flag when component unmounts
// import { onUnmounted } from 'vue'; // Add this import if using onUnmounted
// onUnmounted(() => {
//   authStore.resetRegistrationSuccess();
// });

</script>

<template>
  <div class="auth-container">
    <div class="tabs">
      <button :class="{ active: activeTab === 'login' }" @click="activeTab = 'login'">Login</button>
      <button :class="{ active: activeTab === 'register' }" @click="activeTab = 'register'">Register</button>
    </div>

    <div class="form-container">
      <!-- Login Form -->
      <form v-if="activeTab === 'login'" @submit.prevent="handleLogin" class="auth-form">
        <h2>Login</h2>
        <div v-if="authStore.registrationSuccess" class="success-message">
          Registration successful! Please log in.
        </div>
        <div class="form-group">
          <label for="login-email">Email:</label>
          <input id="login-email" type="email" v-model="loginEmail" required :disabled="authStore.isLoading">
        </div>
        <div class="form-group">
          <label for="login-password">Password:</label>
          <input id="login-password" type="password" v-model="loginPassword" required :disabled="authStore.isLoading">
        </div>
        <div v-if="authStore.isLoading" class="loading-indicator">Logging in...</div>
        <div v-if="authStore.error && !authStore.isLoading" class="error-message">{{ authStore.error }}</div>
        <button type="submit" :disabled="authStore.isLoading">Login</button>
      </form>

      <!-- Registration Form -->
      <form v-if="activeTab === 'register'" @submit.prevent="handleRegister" class="auth-form">
        <h2>Register</h2>
        <div class="form-group">
          <label for="register-email">Email:</label>
          <input id="register-email" type="email" v-model="registerEmail" required :disabled="authStore.isLoading">
        </div>
        <div class="form-group">
          <label for="register-password">Password:</label>
          <input id="register-password" type="password" v-model="registerPassword" required :disabled="authStore.isLoading">
        </div>
        <!-- Optional: Add password confirmation field here -->
        <div v-if="authStore.isLoading" class="loading-indicator">Registering...</div>
        <div v-if="authStore.error && !authStore.isLoading" class="error-message">{{ authStore.error }}</div>
        <button type="submit" :disabled="authStore.isLoading">Register</button>
      </form>
    </div>
  </div>
</template>

<style scoped>
.auth-container {
  max-width: 400px;
  margin: 50px auto;
  padding: 20px;
  border: 1px solid #ccc;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.tabs {
  display: flex;
  margin-bottom: 20px;
  border-bottom: 1px solid #ccc;
}

.tabs button {
  padding: 10px 20px;
  border: none;
  background-color: transparent;
  cursor: pointer;
  font-size: 1em;
  margin-right: 5px;
  border-bottom: 3px solid transparent;
}

.tabs button.active {
  border-bottom: 3px solid #007bff; /* Or your theme color */
  font-weight: bold;
}

.auth-form h2 {
  text-align: center;
  margin-bottom: 20px;
}

.form-group {
  margin-bottom: 15px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
}

.form-group input {
  width: 100%;
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
  box-sizing: border-box; /* Include padding in width */
}

button[type="submit"] {
  width: 100%;
  padding: 10px;
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1em;
}

button[type="submit"]:disabled {
  background-color: #aaa;
  cursor: not-allowed;
}

.loading-indicator,
.error-message,
.success-message {
  text-align: center;
  margin-bottom: 15px;
  padding: 10px;
  border-radius: 4px;
}

.loading-indicator {
  color: #555;
}

.error-message {
  color: #d9534f;
  background-color: #f2dede;
  border: 1px solid #ebccd1;
}

.success-message {
   color: #3c763d;
   background-color: #dff0d8;
   border: 1px solid #d6e9c6;
}
</style>