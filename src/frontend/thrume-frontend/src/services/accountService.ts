import apiClient from '../axiosInstance';

export interface FollowerFollowingUser {
  userName: string;
  pictureUrl: string;
}

export default {
  uploadAvatar(formData: FormData) {
    return apiClient.put('/account/updateProfile', formData, {
      headers: {'Content-Type': 'multipart/form-data'}
    });
  },

  async getFollowers(userName: string): Promise<FollowerFollowingUser[]> {
    try {
      const response = await apiClient.get(`/account/followers/${userName}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching followers:', error);
      throw error;
    }
  },

  async getFollowing(userName: string): Promise<FollowerFollowingUser[]> {
    try {
      const response = await apiClient.get(`/account/following/${userName}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching following:', error);
      throw error;
    }
  }
}