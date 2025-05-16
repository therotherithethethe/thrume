// Axios-based authentication API functions
import apiClient from '@/services/api';
import type { LoginRequest, RegisterRequest, CurrentUser } from '@/types/auth';

export async function apiLogin(credentials: LoginRequest): Promise<void> {
  await apiClient.post('/auth/login?useCookies=true', credentials);
}

export async function apiRegister(credentials: RegisterRequest): Promise<void> {
  await apiClient.post('/auth/register', credentials);
}

export async function apiLogout(): Promise<void> {
  await apiClient.post('/auth/logout');
}

export async function apiGetCurrentUser(): Promise<CurrentUser> {
  //console.log("apiGetCurrentUser exectued");
  const response = await apiClient.get<CurrentUser>('/auth/manage/info');
  console.log("1231231");
  return response.data;
}
