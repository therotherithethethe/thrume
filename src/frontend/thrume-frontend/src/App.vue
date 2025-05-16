<script setup lang="ts">
import { onMounted, ref } from 'vue'; // Added ref
import { useAuthStore } from '@/stores/auth';
import SidebarNav from '@/components/SidebarNav.vue';
import Modal from '@/components/Modal.vue'; // Import Modal
import CreatePostForm from '@/components/CreatePostForm.vue'; // Import CreatePostForm

const authStore = useAuthStore();
const isCreatePostModalOpen = ref(false);

const openCreatePostModal = () => {
  isCreatePostModalOpen.value = true;
};

const closeCreatePostModal = () => {
  isCreatePostModalOpen.value = false;
};

onMounted(() => {

  authStore.initializeAuth();
});
</script>

<template>
  <div class="app-layout">
    <SidebarNav v-if="authStore.isAuthenticated" @open-create-post="openCreatePostModal" />
    <main class="main-content">
      <RouterView />
    </main>

    <Modal :is-open="isCreatePostModalOpen" @close="closeCreatePostModal" title="Create New Post">
      <CreatePostForm @close="closeCreatePostModal" @post-created="closeCreatePostModal" />
    </Modal>
  </div>
</template>

<style scoped>
.app-layout {
  display: flex;
  height: 100vh;
  overflow: hidden; /* Prevent scrollbars on the layout itself */
}

.main-content {
  flex-grow: 1;
  padding: 20px;
  overflow-y: auto; /* Allow scrolling for main content only */
  height: 100vh; /* Ensure it takes full viewport height to enable overflow */
}

/* Basic responsive consideration: hide sidebar on small screens */
/* You might want a more sophisticated hamburger menu for mobile */
@media (max-width: 768px) {
  .app-layout {
    flex-direction: column;
  }
  /* Sidebar would need to be togglable in a real mobile view */
  /* For now, if authenticated, it might stack or you might hide it */
  /* This is a very basic example */
  .main-content {
    width: 100%;
    padding: 10px;
  }
}
</style>
