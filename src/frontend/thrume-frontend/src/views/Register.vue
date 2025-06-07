<!-- views/Auth.vue -->
<script setup lang="ts">
import apiClient from "../axiosInstance"
import { onMounted, ref } from 'vue';
import { useAuthStore } from "../stores/authStore";
import { useAccountStore } from "../stores/accountStore";
import { useRouter } from "vue-router";
import { RawAccountFromApi } from "../types";
import { AxiosError } from "axios";

// Form and Auth State
const email = ref('1lexoradka280211@gmail.com');
const password = ref('123456');
const authStore = useAuthStore();
const accountStore = useAccountStore();
const router = useRouter();
const isLoading = ref(false);

// --- Pop-up State ---
const showPopup = ref(false);
const popupMessage = ref('');
const popupType = ref<'success' | 'error'>('error');

/**
 * Triggers the pop-up with a specific message and type.
 * @param message The message to display.
 * @param type The type of pop-up ('success' or 'error').
 */
function triggerPopup(message: string, type: 'success' | 'error' = 'error') {
  popupMessage.value = message;
  popupType.value = type;
  showPopup.value = true;
}

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

async function handleRegister() {
  if (isLoading.value) return; // Prevent multiple submissions
  isLoading.value = true;
  
  try {
    const registerResponse = await apiClient.post('auth/register', {
      email: email.value,
      password: password.value,
    });

    if (registerResponse.status === 200) {
      // Show success pop-up
      triggerPopup('Registration successful! You can now log in.', 'success');
      // Optionally, clear fields after successful registration
      email.value = '';
      password.value = '';
    }
  } catch (e) {
    // --- ERROR HANDLING & POP-UP TRIGGER ---
    const err = e as AxiosError<{ message?: string, errors?: any }>;
    let errorMessage = "An unknown error occurred. Please try again.";

    // Extract a more specific error message from the API response if available
    if (err.response?.data?.message) {
      errorMessage = err.response.data.message;
    } else if (err.message) {
      errorMessage = err.message;
    }
    
    // Show error pop-up
    triggerPopup(errorMessage, 'error');
  } finally {
    isLoading.value = false;
  }
}

// Redirect to home if already authenticated on component mount
onMounted(async () => {
  const isAuthenticated = await authStore.checkAuth();
  if (isAuthenticated) {
    const accountFetched = await fetchAndStoreCurrentAccount();
    if (accountFetched) {
      router.push({name: 'Home'});
    }
  }
});
</script>

<template>
  <!-- Pop-up Component -->
  <div v-if="showPopup" class="popup-overlay" @click="showPopup = false">
    <div :class="['popup-content', popupType]" @click.stop>
      <h3 class="popup-title">{{ popupType === 'success' ? 'Success' : 'Error' }}</h3>
      <p>{{ popupMessage }}</p>
      <button class="popup-close-btn" @click="showPopup = false">Close</button>
    </div>
  </div>

  <!-- Registration Form -->
  <div class="form-container">
    <div>
      <label for="email">Email:</label>
      <input type="email" name="email" id="email" v-model="email">
    </div>

    <div>
      <label for="password">Password:</label>
      <input type="password" name="password" id="password" v-model="password">
    </div>
    
    <!-- Changed value to "Register" and added disabled state -->
    <input 
      type="button" 
      value="Register" 
      @click="handleRegister"
      :disabled="isLoading"
    >
  </div>
</template>

<style scoped>
/* --- POP-UP STYLES --- */
.popup-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.popup-content {
  background-color: white;
  padding: 25px 30px;
  border-radius: 8px;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
  width: 90%;
  max-width: 400px;
  text-align: center;
}

/* Style for the pop-up title */
.popup-title {
  margin-top: 0;
  font-size: 1.5rem;
  font-weight: 500;
}

/* Conditional border color based on type */
.popup-content.error {
  border-top: 5px solid #e74c3c; /* Red for error */
}
.popup-content.error .popup-title {
  color: #e74c3c;
}

.popup-content.success {
  border-top: 5px solid #2ecc71; /* Green for success */
}
.popup-content.success .popup-title {
  color: #2ecc71;
}

.popup-content p {
  margin: 15px 0;
  font-size: 1rem;
  color: #555;
}

.popup-close-btn {
  background-color: #3498db;
  color: white;
  padding: 10px 25px;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 1rem;
  margin-top: 10px;
  transition: background-color 0.2s ease;
}

.popup-close-btn:hover {
  background-color: #2980b9;
}


/* --- FORM STYLES --- */
.form-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: calc(100vh - 40px);
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

input[type="button"]:hover:not(:disabled) {
  background-color: #0056b3;
}

/* Style for the button when it's disabled */
input[type="button"]:disabled {
  background-color: #a0c4e4;
  cursor: not-allowed;
}
</style>