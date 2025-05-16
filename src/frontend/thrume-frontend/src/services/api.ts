// Axios instance for API calls with cookie and CSRF support
import axios from 'axios';
import { useAuthStore } from '@/stores/auth';
import router from '@/router';

const apiClient = axios.create({
  baseURL: 'https://localhost:5133',
  withCredentials: true
});

let csrfPromise: Promise<void> | null = null;

// Ensure CSRF token is fetched and cookie set
export function ensureCsrfToken(): Promise<void> {
  if (!csrfPromise) {
    csrfPromise = apiClient.get('/antiforgery/token').then(() => {});
  }
  return csrfPromise;
}

// Request interceptor to add CSRF header
apiClient.interceptors.request.use(async (config) => {
  //await ensureCsrfToken();
  const token = document.cookie
    .split('; ')
    .find(row => row.startsWith('MYAPP-XSRF-TOKEN='))
    ?.split('=')[1];
  if (token && config.headers) {
    config.headers['X-XSRF-TOKEN'] = token;
  }
  return config;
});

// Response interceptor for 401 Unauthorized handling
apiClient.interceptors.response.use(
  response => response,
  error => {
    if (error.response && error.response.status === 401) {
      const authStore = useAuthStore();
      authStore.logout();
      if (router.currentRoute.value.name !== 'Auth') {
        router.push({ name: 'Auth', query: { redirect: router.currentRoute.value.fullPath } });
      }
    }
    return Promise.reject(error);
  }
);

export default apiClient;