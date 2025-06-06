import axiosInstance from '../axiosInstance';
import axios from 'axios';
import { Conversation, Message } from '../types';

export interface SendMessageRequest {
  id: { value: string };
  content: string;
}

export interface SendMessageResponse {
  id: { value: string };
  conversationId: { value: string };
  senderId: { value: string };
  content: string;
  timestamp: string;
}

export const getConversations = async (): Promise<Conversation[]> => {
  const response = await axiosInstance.get('/messages/conversations');
  return response.data;
};

export const getMessages = async (conversationId: string, page: number = 1, pageSize: number = 20): Promise<Message[]> => {
  const response = await axiosInstance.get(`/messages/conversations/${conversationId}`, {
    params: { page, pageSize }
  });
  return response.data;
};

export const startConversation = async (userName: string): Promise<Conversation> => {
  try {
    const response = await axiosInstance.post(`/messages/conversations/start/${userName}`);
    
    if (response.status === 200) {
      return response.data;
    } else if (response.status === 400) {
      throw new Error(response.data?.message || 'Failed to start conversation');
    } else {
      throw new Error('Unexpected response status');
    }
  } catch (error: any) {
    console.error('Error starting conversation:', error);
    throw error;
  }
};

export const deleteConversation = async (conversationId: string): Promise<void> => {
  try {
    const response = await axiosInstance.delete(`/messages/conversations/${conversationId}`);
    
    if (response.status === 200) {
      console.log('Conversation deleted successfully');
      return;
    } else if (response.status === 400) {
      console.error('Failed to delete conversation:', response.data);
      throw new Error('Failed to delete conversation: ' + (response.data.message || 'Unknown error'));
    } else {
      throw new Error('Unexpected response status: ' + response.status);
    }
  } catch (error: any) {
    console.error('Error deleting conversation:', error);
    if (error.response?.status === 400) {
      throw new Error('Failed to delete conversation: ' + (error.response.data?.message || 'Permission denied or conversation not found'));
    }
    throw new Error('Failed to delete conversation: ' + error.message);
  }
};

export async function sendMessage(conversationId: string, content: string): Promise<SendMessageResponse> {
  try {
    console.log('Sending message to conversation:', conversationId, 'with content:', content);

    const requestBody: SendMessageRequest = {
      id: { value: conversationId },
      content: content
    };

    const response = await axiosInstance.post<SendMessageResponse>('/messages/conversations', requestBody);
    
    console.log('Message sent successfully:', response.data);
    return response.data;
  } catch (error) {
    console.error('Error sending message:', error);
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 400) {
        throw new Error('Invalid message data provided');
      } else if (error.response?.status === 403) {
        throw new Error('Not authorized to send messages to this conversation');
      } else if (error.response?.status === 404) {
        throw new Error('Conversation not found');
      } else if (error.response?.status === 500) {
        throw new Error('Server error occurred while sending message');
      } else {
        throw new Error(`Failed to send message: ${error.response?.data?.message || error.message}`);
      }
    }
    throw new Error('Network error occurred while sending message');
  }
}