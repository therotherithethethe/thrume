// Axios-based post API functions
import apiClient from '@/services/api';

export interface PostId {
  value: string;
}

export interface PostImage {
  url: string;
}

export interface AuthorInfo {
  id?: string;
  username: string;
  avatarUrl?: string | null;
}

export interface Post {
  id: PostId;
  content: string;
  images: PostImage[];
  likedBy: string[];
  createdAt: string;
  author: AuthorInfo;
  commentsCount?: number;
}

// Fetch posts for a specific user
export async function apiGetUserPosts(username: string): Promise<Post[]> {
  const response = await apiClient.get<Post[]>(`/posts/${username}`);
  return response.data;
}

// Create a new post
export async function apiCreatePost(formData: FormData): Promise<Post> {
  const response = await apiClient.post<Post>('/posts', formData);
  return response.data;
}

// Like a post
export async function apiLikePost(postIdValue: string): Promise<void> {
  await apiClient.put('/posts/like', { value: postIdValue });
}