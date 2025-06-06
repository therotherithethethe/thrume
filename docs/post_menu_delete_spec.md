# Post Menu and Delete Functionality Specification

## Overview
Implement a 3-dot menu in the top-right corner of each post with:
- Menu visible to all users
- Delete option (red button) visible only to post owners
- Confirmation dialog before deletion
- API call to delete post

## API Service Method

### postService.ts
```typescript:src/frontend/thrume-frontend/src/services/postService.ts
export const deletePostById = (postId: string) => {
  return axiosInstance.delete('/posts', {
    data: { value: postId }
  });
};
```

## Component Changes

### PostCard.vue Template
```html:src/frontend/thrume-frontend/src/components/PostCard.vue
<template>
  <article class="post-card">
    <!-- ... existing content ... -->
    
    <header class="post-header">
      <!-- ... existing author info ... -->
      
      <div class="post-actions-menu">
        <button class="menu-button" @click.stop="showMenu = !showMenu">⋮</button>
        <div class="menu-dropdown" v-if="showMenu" v-click-outside="closeMenu">
          <button v-if="isPostOwner" class="delete-button" @click="confirmDelete">
            Delete
          </button>
          <button v-else class="menu-item">Report</button>
        </div>
      </div>
    </header>
    
    <!-- ... rest of template ... -->
  </article>
</template>
```

### PostCard.vue Script
```typescript:src/frontend/thrume-frontend/src/components/PostCard.vue
import { deletePostById } from '../services/postService';

// Add inside setup()
const showMenu = ref(false);
const showDeleteDialog = ref(false);

const isPostOwner = computed(() => {
  return accountStore.currentAccount?.id === props.post.author?.id;
});

const closeMenu = () => {
  showMenu.value = false;
};

const confirmDelete = () => {
  showDeleteDialog.value = true;
  showMenu.value = false;
};

const deletePost = async () => {
  try {
    await deletePostById(props.post.id);
    emit('post-deleted', props.post.id);
  } catch (error) {
    console.error('Error deleting post:', error);
  } finally {
    showDeleteDialog.value = false;
  }
};
```

### Delete Confirmation Dialog
```html:src/frontend/thrume-frontend/src/components/PostCard.vue
<dialog v-if="showDeleteDialog" class="confirmation-dialog">
  <p>Are you sure you want to delete this post?</p>
  <div class="dialog-buttons">
    <button @click="showDeleteDialog = false">Cancel</button>
    <button class="confirm-delete" @click="deletePost">Delete</button>
  </div>
</dialog>
```

## Style Additions
```css:src/frontend/thrume-frontend/src/components/PostCard.vue
.post-actions-menu {
  position: relative;
  margin-left: auto;
}

.menu-button {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  padding: 5px;
  color: #555;
}

.menu-dropdown {
  position: absolute;
  right: 0;
  background: white;
  border: 1px solid #ddd;
  border-radius: 4px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  z-index: 100;
}

.menu-item, .delete-button {
  display: block;
  width: 100%;
  padding: 8px 16px;
  text-align: left;
  background: none;
  border: none;
  cursor: pointer;
}

.delete-button {
  color: #ff0000;
}

.menu-item:hover, .delete-button:hover {
  background-color: #f5f5f5;
}

.confirmation-dialog {
  position: fixed;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  padding: 20px;
  border: 1px solid #ccc;
  border-radius: 8px;
  z-index: 1000;
}

.dialog-buttons {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 15px;
}

.confirm-delete {
  background-color: #ff0000;
  color: white;
}
```

## Validation Checklist
- [ ] 3-dot menu appears in top-right corner of each post
- [ ] Menu shows delete option only for post owner
- [ ] Delete option is red
- [ ] Clicking delete shows confirmation dialog
- [ ] API call made on confirmation
- [ ] Post removed from UI after deletion
- [ ] Menu closes when clicking outside