import apiClient from '../axiosInstance';

export interface FollowerFollowingUser {
  userName: string;
  pictureUrl: string;
}

export default {
  uploadAvatar(formData: FormData) {
    return apiClient.put('/api/account/updateProfile', formData, {
      headers: {'Content-Type': 'multipart/form-data'}
    });
  },

  async getFollowers(userName: string): Promise<FollowerFollowingUser[]> {
    try {
      const response = await apiClient.get(`/api/account/followers/${userName}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching followers:', error);
      throw error;
    }
  },

  async getFollowing(userName: string): Promise<FollowerFollowingUser[]> {
    try {
      const response = await apiClient.get(`/api/account/following/${userName}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching following:', error);
      throw error;
    }
  }
}

export async function searchUsers(
  userName: string,
  page: number = 1,
  pageSize: number = 10
): Promise<FollowerFollowingUser[]> {
  try {
    const response = await apiClient.get(`/api/account/search/${userName}`, {
      params: { page, pageSize }
    });
    return response.data;
  } catch (error) {
    console.error('Error searching users:', error);
    throw error;
  }
}