import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import { createPinia } from 'pinia';
import { ensureCsrfToken } from '@/services/api';
import { useAuthStore } from '@/stores/auth';

const app = createApp(App);
  app.use(createPinia());
  app.use(router);
  await ensureCsrfToken();
  const authStore = useAuthStore();
  await authStore.initializeAuth();
  app.mount('#app');