# Messaging Feature Implementation Plan

## Overview
Implement messaging functionality allowing users to view conversations when clicking "Messages" in sidebar.

## Components
1. **MessagesView.vue** - Main view for conversations
2. **ConversationCard.vue** - Individual conversation display
3. **MessageService.ts** - API integration layer

## API Integration
```typescript
// Endpoint: GET /messages/conversations
// Response: Conversation[]
export interface Conversation {
  id: { value: string };
  createdAt: string;
  participants: Participant[];
}

export interface Participant {
  id: { value: string };
  userName: string;
  pictureUrl: string | null;
}
```

## Implementation Steps

### 1. Add Types
```typescript:src/types/index.ts
export interface Conversation {
  id: { value: string };
  createdAt: string;
  participants: Participant[];
}

export interface Participant {
  id: { value: string };
  userName: string;
  pictureUrl: string | null;
}
```

### 2. Create Message Service
```typescript:src/services/messageService.ts
import axiosInstance from '../axiosInstance';
import { Conversation } from '../types';

export const getConversations = async (): Promise<Conversation[]> => {
  const response = await axiosInstance.get('/messages/conversations');
  return response.data;
};
```

### 3. Create MessagesView Component
```vue:src/views/MessagesView.vue
<script setup lang="ts">
import { ref } from 'vue';
import { getConversations } from '@/services/messageService';
import { Conversation } from '@/types';

const conversations = ref<Conversation[]>([]);

getConversations().then(data => conversations.value = data);
</script>

<template>
  <div class="messages-view">
    <h1>Your Conversations</h1>
    <div v-for="convo in conversations" :key="convo.id.value">
      <div v-for="participant in convo.participants" 
           v-if="participant.id.value !== currentUserId">
        {{ participant.userName }}
      </div>
    </div>
  </div>
</template>
```

### 4. Add Route Configuration
```typescript:src/router/index.ts
import MessagesView from '@/views/MessagesView.vue';

const routes = [
  // ...existing routes
  {
    path: '/messages',
    name: 'Messages',
    component: MessagesView,
    meta: { requiresAuth: true }
  }
];
```

### 5. Update Sidebar Navigation
```vue:src/components/Sidebar.vue
<template>
  <nav>
    <!-- ...existing links -->
    <router-link to="/messages">
      <i class="message-icon"></i> Messages
    </router-link>
  </nav>
</template>
```

### 6. Next Steps
- Implement ConversationCard component
- Add pagination support
- Create message thread view
- Implement real-time updates