// stores/accountStore.ts
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import apiClient from '../axiosInstance';
import { RawAccountFromApi, Account } from '../types';

import accountService from '../services/accountService';

export const useAccountStore = defineStore('account', () => {
  // State: Use ref for reactive properties
  const currentAccount = ref<Account | null>(null);
  const loading = ref(false); // For loading `currentAccount`
  const error = ref<string | null>(null); // For `currentAccount` errors

  // Getters: Use computed for derived state
  const isLoggedIn = computed(() => !!currentAccount.value);
  const accountId = computed(() => currentAccount.value?.id || null);
  const accountUsername = computed(() => currentAccount.value?.username || 'Guest');
  const accountProfilePicture = computed(() => currentAccount.value?.profilePictureUrl || null);

  // Actions
  const updateProfilePicture = async (file: File) => {
    if (!currentAccount.value) {
      throw new Error('No account found');
    }
    
    const formData = new FormData();
    formData.append('file', file);
    
    try {
      await accountService.uploadAvatar(formData);
      
      // Optimistically update profile picture
      if (currentAccount.value) {
        // Create a new object to trigger reactivity
        currentAccount.value = {
          ...currentAccount.value,
          profilePictureUrl: URL.createObjectURL(file)
        };
      }
      
      // Refresh account data
      await fetchAccountById(currentAccount.value!.id);
    } catch (err: any) {
      console.error('Failed to update profile picture:', err);
      throw err;
    }
  };

  /**
   * Transforms raw account data from API into the desired Account interface.
   * This is for a *full* account object, not a partial embedded one.
   */
  const transformRawAccount = (rawAccount: RawAccountFromApi): Account => {
    return {
      id: rawAccount.id.value,
      username: rawAccount.userName, // Mapping userName to username for consistency
      email: rawAccount.email,
      profilePictureUrl: rawAccount.pictureUrl,
      posts: rawAccount.posts.map(p => p.value),
      likedPosts: rawAccount.likedPosts.map(lp => lp.value),
    };
  };

  /**
   * Sets the current account in the store. Useful after login or initial load.
   * Accepts raw API data and transforms it.
   */
  const setAccount = (rawAccount: RawAccountFromApi) => {
    currentAccount.value = transformRawAccount(rawAccount); // Use .value for ref
    error.value = null; // Use .value for ref
  };

  /**
   * Clears the current account, effectively "logging out".
   */
  const clearAccount = () => {
    currentAccount.value = null; // Use .value for ref
    error.value = null; // Use .value for ref
    loading.value = false; // Use .value for ref
  };

  /**
   * Fetches account data by ID from the API and sets it as the current account.
   */
  const fetchAccountById = async (id: string) => { // Renamed parameter from accountId to id for clarity
    loading.value = true; // Use .value for ref
    error.value = null; // Use .value for ref
    currentAccount.value = null; // Clear existing account while loading

    try {
      const response = await apiClient.get<RawAccountFromApi>(`/accounts/${id}`);
      setAccount(response.data); // Call local action
    } catch (err: any) {
      console.error('Failed to fetch current account:', err);
      error.value = 'Failed to load account data. Please try again.'; // Use .value for ref
      if (err.response && err.response.status === 404) {
        error.value = 'Account not found.'; // Use .value for ref
      }
    } finally {
      loading.value = false; // Use .value for ref
    }
  };

  /**
   * Fetches account data by username from the API and sets it as the current account.
   */
  const fetchAccountByUsername = async (usernameParam: string) => { // Renamed parameter for clarity
    loading.value = true;
    error.value = null;
    currentAccount.value = null;

    try {
      const response = await apiClient.get<RawAccountFromApi>(`/accounts/username/${usernameParam}`);
      setAccount(response.data);
    } catch (err: any) {
      console.error('Failed to fetch account by username:', err);
      error.value = 'Failed to load account data by username. Please try again.';
      if (err.response && err.response.status === 404) {
        error.value = `Account with username "${usernameParam}" not found.`;
      }
    } finally {
      loading.value = false;
    }
  };

  // Return all state, getters, and actions that should be public
  return {
    currentAccount,
    loading,
    error,
    isLoggedIn,
    accountId,
    accountUsername,
    accountProfilePicture,
    setAccount,
    clearAccount,
    fetchAccountById,
    fetchAccountByUsername,
    updateProfilePicture,
  };
});