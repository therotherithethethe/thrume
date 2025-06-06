import axiosInstance from '../axiosInstance';
import { CreatePostRequest } from '../types';

export const createPost = async (postData: CreatePostRequest): Promise<void> => {
  const formData = new FormData();
  formData.append('Content', postData.content);
  postData.images.forEach(image => {
    formData.append('Images', image);
  });
  
  await axiosInstance.post('/posts', formData, {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  });
};

export const likePost = (postId: string) => {
  return axiosInstance.put('/posts/like', { value: postId });
};

export const unlikePost = (postId: string) => {
  return axiosInstance.put('/posts/unlike', { value: postId });
};

export const deletePostById = (postId: string) => {
  return axiosInstance.delete('/posts', {
    data: { value: postId }
  });
};

// Create a new comment for a post
export const createComment = (postId: string, content: string) => {
  return axiosInstance.post('/comments', {
    postId: { value: postId },
    content,
    parentCommentId: null
  });
};