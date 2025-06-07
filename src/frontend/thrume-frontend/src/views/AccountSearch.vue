<template>
  <div class="account-search">
    <!-- Search Bar -->
    <div class="search-bar">
      <input 
        v-model="searchTerm" 
        placeholder="Search for users..."
        @keyup.enter="performSearch"
      />
      <button @click="performSearch">Search</button>
    </div>

    <!-- Results Header -->
    <h1 v-if="route.params.name">Search Results for "{{ route.params.name }}"</h1>
    
    <!-- Loading State -->
    <div v-if="loading" class="loading-indicator">
      Loading...
    </div>
    
    <!-- Error State -->
    <div v-if="error" class="error-message">
      An error occurred: {{ error.message }}
    </div>
    
    <!-- Results List -->
    <div v-if="paginatedResults.length > 0" class="results-container">
      <!-- Changed from a grid to a vertical list -->
      <div v-for="user in paginatedResults" :key="user.userName" class="user-list-item">
        <div class="profile-pic-container">
          <img v-if="user.pictureUrl" :src="user.pictureUrl" alt="Profile" class="profile-pic" />
          <!-- Placeholder SVG for users without a picture -->
          <svg v-else width="50" height="50" viewBox="0 0 24 24" class="profile-pic-placeholder">
              <circle cx="12" cy="12" r="11" fill="#e9ecef"/>
              <path d="M12 12m-4 0a4 4 0 1 0 8 0a4 4 0 1 0 -8 0" fill="#adb5bd"/>
              <path d="M12,13 C8,13 6,15 6,17 L18,17 C18,15 16,13 12,13 Z" fill="#adb5bd"/>
          </svg>
        </div>
        <div class="user-info" @click="router.push(`/${user.userName}`)">
          <span class="user-name">{{ user.userName }}</span>
        </div>
      </div>
    </div>
    
    <!-- No Results Message -->
    <div v-else-if="!loading && results.length === 0 && route.params.name">
      <p class="no-results">No users found for "{{ route.params.name }}".</p>
    </div>

    <!-- Pagination Controls -->
    <div v-if="totalPages > 1" class="pagination-controls">
      <div class="page-size-selector">
        <label for="pageSize">Show: </label>
        <select id="pageSize" v-model="pageSize">
          <option>5</option>
          <option>10</option>
          <option>20</option>
          <option>50</option>
        </select>
      </div>
      <div class="page-nav">
        <button @click="prevPage" :disabled="currentPage <= 1">Previous</button>
        <span>Page {{ currentPage }} of {{ totalPages }}</span>
        <button @click="nextPage" :disabled="currentPage >= totalPages">Next</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { searchUsers, FollowerFollowingUser } from '../services/accountService';

const route = useRoute();
const router = useRouter();
const loading = ref(false);
const error = ref<Error | null>(null);
const results = ref<FollowerFollowingUser[]>([]);
const searchTerm = ref(route.params.name as string || '');
const currentPage = ref(1);
const pageSize = ref(10);

const totalPages = computed(() => Math.ceil(results.value.length / Number(pageSize.value)));
const paginatedResults = computed(() => {
  const start = (currentPage.value - 1) * Number(pageSize.value);
  const end = start + Number(pageSize.value);
  return results.value.slice(start, end);
});

const fetchData = async () => {
  if (!searchTerm.value) return;
  loading.value = true;
  error.value = null;
  try {
    results.value = await searchUsers(searchTerm.value);
    currentPage.value = 1; // Reset to first page on new search
  } catch (err) {
    error.value = err as Error;
  } finally {
    loading.value = false;
  }
};

const performSearch = () => {
  console.log(searchTerm.value.trim());
  if (searchTerm.value.trim()) {
    router.push(`/search/${searchTerm.value.trim()}`);
  }
};

onMounted(() => {
  fetchData();
});

watch(() => route.params.name, (newName) => {
  fetchData();
});

watch(pageSize, () => {
  if (currentPage.value > totalPages.value) {
    currentPage.value = totalPages.value || 1;
  }
});

const nextPage = () => {
  if (currentPage.value < totalPages.value) {
    currentPage.value++;
  }
};

const prevPage = () => {
  if (currentPage.value > 1) {
    currentPage.value--;
  }
};
</script>

<style scoped>
/* Main container for centering and spacing */
.account-search {
  max-width: 800px;
  margin: 40px auto;
  padding: 20px;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
  color: #333;
}

/* Search bar styling */
.search-bar {
  display: flex;
  margin-bottom: 25px;
}

.search-bar input {
  flex-grow: 1;
  border: 1px solid #ccc;
  padding: 10px 15px;
  font-size: 16px;
  border-radius: 6px 0 0 6px;
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.search-bar input:focus {
  border-color: #007bff;
  box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.25);
}

.search-bar button {
  padding: 10px 20px;
  font-size: 16px;
  background-color: #007bff;
  color: white;
  border: 1px solid #007bff;
  border-radius: 0 6px 6px 0;
  cursor: pointer;
  transition: background-color 0.2s;
}

.search-bar button:hover {
  background-color: #0056b3;
}

/* Header styling */
h1 {
  font-size: 24px;
  font-weight: 300;
  margin-bottom: 20px;
  border-bottom: 1px solid #eee;
  padding-bottom: 10px;
}

/* Status messages */
.loading-indicator,
.error-message,
.no-results {
  text-align: center;
  padding: 40px 20px;
  color: #6c757d;
}

.error-message {
  color: #dc3545;
}

/* --- VERTICAL LIST STYLES --- */

/* Container for the list */
.results-container {
  display: flex;
  flex-direction: column; /* This is the key change for vertical layout */
  gap: 8px; /* Space between list items */
  margin: 20px 0;
}

/* Individual list item */
.user-list-item {
  display: flex;
  align-items: center;
  gap: 15px; /* Space between picture and user info */
  padding: 12px;
  border-radius: 8px;
  border: 1px solid transparent;
  transition: background-color 0.2s, border-color 0.2s;
}

.user-list-item:hover {
  background-color: #f8f9fa;
  border-color: #e9ecef;
  cursor: pointer; /* Suggests the item is clickable */
}

/* Profile picture container and image */
.profile-pic-container {
  flex-shrink: 0;
}

.profile-pic {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  object-fit: cover;
  background-color: #eee;
}

.profile-pic-placeholder {
  width: 50px;
  height: 50px;
  border-radius: 50%;
}

/* User information */
.user-info {
  display: flex;
  flex-direction: column;
}

.user-name {
  font-size: 16px;
  font-weight: 500;
}

/* Pagination controls styling */
.pagination-controls {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 30px;
  padding-top: 20px;
  border-top: 1px solid #eee;
  font-size: 14px;
  color: #6c757d;
}

.page-size-selector, .page-nav {
  display: flex;
  align-items: center;
  gap: 10px;
}

.pagination-controls select {
  padding: 5px;
  border-radius: 4px;
  border: 1px solid #ccc;
}

.pagination-controls button {
  padding: 6px 12px;
  border: 1px solid #ccc;
  background-color: white;
  border-radius: 4px;
  cursor: pointer;
}

.pagination-controls button:hover:not(:disabled) {
  background-color: #f8f9fa;
}

.pagination-controls button:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}
</style>