<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { getConversations, deleteConversation } from '../services/messageService';
import { Conversation, Participant } from '../types';
import { useAccountStore } from '../stores/accountStore';
import DeleteConfirmationModal from '../components/DeleteConfirmationModal.vue';

const router = useRouter();
const route = useRoute();
const accountStore = useAccountStore();
const conversations = ref<Conversation[]>([]);

// State for deletion functionality
const showDeleteModal = ref(false);
const conversationToDelete = ref<Conversation | null>(null);
const isDeletingConversation = ref(false);

onMounted(async () => {
  try {
    conversations.value = await getConversations();
  } catch (error) {
    console.error('Failed to fetch conversations:', error);
  }
});

function getOtherParticipant(participants: Participant[]) {
  return participants.find(p => p.id.value !== accountStore.accountId);
}

function navigateToConversation(conversationId: string) {
  router.push({ name: 'Conversation', params: { conversationId } });
}

// Delete functionality methods
function openDeleteConfirmation(conversation: Conversation, event: Event) {
  event.stopPropagation(); // Prevent navigation when clicking delete button
  conversationToDelete.value = conversation;
  showDeleteModal.value = true;
}

function closeDeleteConfirmation() {
  showDeleteModal.value = false;
  conversationToDelete.value = null;
}

async function confirmDelete() {
  if (!conversationToDelete.value) return;

  isDeletingConversation.value = true;
  
  try {
    await deleteConversation(conversationToDelete.value.id.value);
    handleDeleteSuccess();
  } catch (error) {
    console.error('Failed to delete conversation:', error);
    // Keep modal open on error to allow retry
  } finally {
    isDeletingConversation.value = false;
  }
}

function handleDeleteSuccess() {
  if (!conversationToDelete.value) return;

  const deletedConversationId = conversationToDelete.value.id.value;
  
  // Remove deleted conversation from local array
  conversations.value = conversations.value.filter(
    conv => conv.id.value !== deletedConversationId
  );

  // If currently viewing the deleted conversation, redirect to messages list
  if (route.params.conversationId === deletedConversationId) {
    router.push({ name: 'Messages' });
  }

  // Close modal and reset state
  closeDeleteConfirmation();
}
</script>

<template>
  <div class="messages-view">
    <h1>Your Conversations</h1>
    
    <div v-if="conversations.length === 0" class="empty-state">
      No conversations found
    </div>

    <div v-else>
      <div
        v-for="convo in conversations"
        :key="convo.id.value"
        class="conversation-card"
        @click="navigateToConversation(convo.id.value)"
      >
        <div class="participant-info">
          <span class="username">{{ getOtherParticipant(convo.participants)?.userName }}</span>
          <span class="timestamp">{{ new Date(convo.createdAt).toLocaleString() }}</span>
        </div>
        <button
          @click="openDeleteConfirmation(convo, $event)"
          class="delete-button"
          aria-label="Delete conversation"
          title="Delete conversation"
        >
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor">
            <polyline points="3,6 5,6 21,6"></polyline>
            <path d="m19,6v14a2,2 0 0,1-2,2H7a2,2 0 0,1-2-2V6m3,0V4a2,2 0 0,1,2-2h4a2,2 0 0,1,2,2v2"></path>
            <line x1="10" y1="11" x2="10" y2="17"></line>
            <line x1="14" y1="11" x2="14" y2="17"></line>
          </svg>
        </button>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <DeleteConfirmationModal
      :isOpen="showDeleteModal"
      :isLoading="isDeletingConversation"
      title="Delete Conversation"
      :message="`Are you sure you want to delete your conversation with ${conversationToDelete ? getOtherParticipant(conversationToDelete.participants)?.userName : ''}? This action cannot be undone and all messages will be permanently lost.`"
      confirmText="Delete"
      cancelText="Cancel"
      @confirm="confirmDelete"
      @cancel="closeDeleteConfirmation"
    />
  </div>
</template>

<style scoped>
.messages-view {
  padding: 20px;
}

.conversation-card {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 15px;
  margin-bottom: 15px;
  cursor: pointer;
  transition: background-color 0.2s;
  position: relative;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.conversation-card:hover {
  background-color: #f5f5f5;
}

.participant-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
  flex: 1;
}

.username {
  font-weight: bold;
  color: #333;
}

.timestamp {
  font-size: 0.8rem;
  color: #666;
}

.delete-button {
  background: none;
  border: none;
  color: #666;
  cursor: pointer;
  padding: 8px;
  border-radius: 4px;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  margin-left: 10px;
}

.delete-button:hover {
  background-color: #fee;
  color: #e74c3c;
  transform: scale(1.1);
}

.delete-button:focus {
  outline: 2px solid #e74c3c;
  outline-offset: 2px;
}

.delete-button svg {
  width: 16px;
  height: 16px;
}

.empty-state {
  text-align: center;
  padding: 40px;
  color: #666;
}

/* Mobile responsiveness */
@media (max-width: 768px) {
  .messages-view {
    padding: 15px;
  }
  
  .conversation-card {
    padding: 12px;
  }
  
  .participant-info {
    gap: 2px;
  }
  
  .username {
    font-size: 0.9rem;
  }
  
  .timestamp {
    font-size: 0.75rem;
  }
  
  .delete-button {
    padding: 6px;
  }
  
  .delete-button svg {
    width: 14px;
    height: 14px;
  }
}
</style>