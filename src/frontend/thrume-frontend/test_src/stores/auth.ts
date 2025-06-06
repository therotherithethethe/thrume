import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { apiLogin, apiRegister, apiGetCurrentUser, apiLogout } from '@/services/authApi';
import type { LoginRequest, RegisterRequest, CurrentUser } from '@/types/auth';

export const useAuthStore = defineStore('auth', () => {
    const currentUser = ref<CurrentUser | null>(null);
    const isLoading = ref<boolean>(false);
    const error = ref<string | null>(null);
    const registrationSuccess = ref<boolean>(false);
    const router = useRouter();

    const isAuthenticated = computed(() => !!currentUser.value);

    function clearError() {
        error.value = null;
    }

    function resetRegistrationSuccess() {
        registrationSuccess.value = false;
    }

    async function fetchCurrentUser() {
        isLoading.value = true;
        error.value = null;
        try {
            const user = await apiGetCurrentUser();
            //console.log("what")
            currentUser.value = user;
        } catch (err: any) {
            console.error("Error in fetchCurrentUser:", err);
            error.value = err.message || "Failed to fetch user status.";
            currentUser.value = null;
        } finally {
            isLoading.value = false;
        }
    }

    async function login(credentials: LoginRequest): Promise<boolean> {
        isLoading.value = true;
        error.value = null;
        registrationSuccess.value = false;
        try {
            await apiLogin(credentials);
            await fetchCurrentUser();
            isLoading.value = false;
            return !!currentUser.value;
        } catch (err: any) {
            currentUser.value = null;
            error.value = err.message || 'Login failed. Please check your credentials.';
            isLoading.value = false;
            return false;
        }
    }

    async function register(credentials: RegisterRequest): Promise<boolean> {
        isLoading.value = true;
        error.value = null;
        registrationSuccess.value = false;
        try {
            await apiRegister(credentials);
            registrationSuccess.value = true;
            isLoading.value = false;
            return true;
        } catch (err: any) {
            error.value = err.message || 'Registration failed. Please check your input or try a different email.';
            isLoading.value = false;
            return false;
        }
    }

    async function logout() {
        try {
            await apiLogout();
        } catch (err: any) {
            console.error("Logout failed:", err);
        }
        currentUser.value = null;
        error.value = null;
        registrationSuccess.value = false;
        router.push({ name: 'Auth' });
    }

    async function initializeAuth() {
        await fetchCurrentUser();
    }

    function updateUserAvatar(avatarUrl: string) {
        if (currentUser.value) {
            currentUser.value.avatarUrl = avatarUrl;
        }
    }

    return {
        currentUser,
        isLoading,
        error,
        registrationSuccess,
        isAuthenticated,
        login,
        register,
        logout,
        initializeAuth,
        clearError,
        resetRegistrationSuccess,
        updateUserAvatar,
        fetchCurrentUser
    };
});