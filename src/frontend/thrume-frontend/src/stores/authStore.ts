import { defineStore } from 'pinia'
import { ref } from 'vue';
import apiClient from '../axiosInstance';

export type AuthStatusResponse = {
    isAuthenticated: boolean
}
export const useAuthStore = defineStore('Auth', () => {
    const currentUser = ref<AuthStatusResponse>({
        isAuthenticated: false
    });

    const checkAuth = async (): Promise<boolean> => {
        try {
            const response = await apiClient.get<AuthStatusResponse>('auth/status');
            currentUser.value.isAuthenticated = response.data?.isAuthenticated;
        } catch (e) {
            currentUser.value.isAuthenticated = false
        }
        console.log(`user is authed ${currentUser.value.isAuthenticated}`)
        return currentUser.value.isAuthenticated
    }
    return {currentUser, checkAuth}
})