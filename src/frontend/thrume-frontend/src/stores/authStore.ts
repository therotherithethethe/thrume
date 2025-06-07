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
        
        return currentUser.value.isAuthenticated
    }

    const checkAuthSync = (): boolean => {
        try {
            apiClient.get<AuthStatusResponse>('auth/status').then(resp => {
                currentUser.value.isAuthenticated = resp.data?.isAuthenticated;
            }).catch(e => {
                currentUser.value.isAuthenticated = false
            });
        } catch (e) {
            currentUser.value.isAuthenticated = false
        }
        
        return currentUser.value.isAuthenticated
    }
    return {currentUser, checkAuth, checkAuthSync}
})