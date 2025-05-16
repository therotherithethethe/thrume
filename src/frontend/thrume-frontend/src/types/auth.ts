// Define shared authentication-related interfaces

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
}

// Define interface for the current authenticated user
export interface CurrentUser {
  id: string; // Or Guid, depending on your backend
  username: string;
  email?: string;
  avatarUrl?: string | null;
  // Add other relevant user fields
}