# Avatar Upload Implementation Plan

## 1. Feature Overview
Implement avatar upload functionality where:
- Clicking profile avatar triggers file selection
- Supported formats: JPG, PNG, JPEG
- Max file size: 5MB
- Uses PUT `/account/updateProfile` endpoint
- Provides visual feedback during upload

## 2. Components to Modify

### ProfileHeader.vue
- Add hidden file input with `accept="image/jpeg,image/png"`
- Add click handler to avatar that triggers file input
- Add `uploadAvatar` method to:
  ```ts
  async function uploadAvatar(file: File) {
    // Validate file
    if (!['image/jpeg', 'image/png'].includes(file.type)) {
      showError('Invalid file type');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      showError('File too large (max 5MB)');
      return;
    }
    
    // Show loading state
    setAvatarLoading(true);
    
    try {
      await accountStore.updateProfilePicture(file);
      emit('avatar-updated');
    } catch (error) {
      showError('Upload failed');
    } finally {
      setAvatarLoading(false);
    }
  }
  ```

### accountStore.ts
- Add updateProfilePicture method:
  ```ts
  const updateProfilePicture = async (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    await accountService.uploadAvatar(formData);
    await fetchAccountById(currentAccount.value!.id); // Refresh data
  }
  ```

### AccountPosts.vue
- Add handler for 'avatar-updated' event:
  ```ts
  const handleAvatarUpdated = () => {
    fetchProfileData(username.value);
  }
  ```

## 3. New Files to Create

### src/services/accountService.ts
```ts
import apiClient from './axiosInstance';

export default {
  uploadAvatar(formData: FormData) {
    return apiClient.put('/account/updateProfile', formData, {
      headers: {'Content-Type': 'multipart/form-data'}
    });
  }
}
```

## 4. Validation Requirements
- Client-side validation:
  - File type (must be image/jpeg or image/png)
  - File size (≤5MB)
- Server should also validate and return appropriate errors

## 5. Visual Feedback
- During upload:
  - Show loading spinner overlay on avatar
  - Disable click events
- After success:
  - Immediately update avatar preview
  - Show success toast
- On error:
  - Show error toast with message
  - Revert to previous avatar

## 6. Testing Checklist
- [ ] Avatar click triggers file dialog
- [ ] Valid files are accepted
- [ ] Invalid files show error
- [ ] Files >5MB show error
- [ ] Upload shows loading state
- [ ] Success updates avatar globally
- [ ] Errors are properly handled
- [ ] Profile data refreshes after update