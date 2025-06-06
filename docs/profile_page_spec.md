# Profile Page Specification

## Overview
Implement a comprehensive profile page at `/:name` route with:
- Profile header section
- Post list in existing style
- API integration for profile data
- Graceful handling of null profile pictures

## Profile Header Component

### ProfileHeader.vue
```vue:src/frontend/thrume-frontend/src/components/ProfileHeader.vue
<template>
  <div class="profile-header">
    <div class="profile-picture-container">
      <img v-if="pictureUrl" :src="pictureUrl" class="profile-picture">
      <div v-else class="profile-picture-placeholder">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="white">
          <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
        </svg>
      </div>
    </div>
    
    <div class="profile-info">
      <h1 class="username">{{ username }}</h1>
      
      <div class="stats">
        <div class="stat">
          <span class="count">{{ postCount }}</span>
          <span class="label">Posts</span>
        </div>
        <div class="stat">
          <span class="count">{{ followersCount }}</span>
          <span class="label">Followers</span>
        </div>
        <div class="stat">
          <span class="count">{{ followingCount }}</span>
          <span class="label">Following</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps({
  username: String,
  pictureUrl: String,
  postCount: Number,
  followersCount: Number,
  followingCount: Number
});
</script>

<style scoped>
.profile-header {
  display: flex;
  align-items: center;
  padding: 20px;
  margin-bottom: 30px;
}

.profile-picture-container {
  width: 150px;
  height: 150px;
  margin-right: 30px;
}

.profile-picture {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
  border: 1px solid #eee;
}

.profile-picture-placeholder {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  background: linear-gradient(45deg, #833ab4, #fd1d1d, #fcb045);
  display: flex;
  align-items: center;
  justify-content: center;
}

.profile-picture-placeholder svg {
  width: 80px;
  height: 80px;
}

.profile-info {
  flex-grow: 1;
}

.username {
  font-size: 2rem;
  margin: 0 0 15px 0;
  color: #333;
}

.stats {
  display: flex;
  gap: 30px;
}

.stat {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.count {
  font-size: 1.5rem;
  font-weight: bold;
}

.label {
  font-size: 0.9rem;
  color: #777;
}
</style>
```

## AccountPosts.vue Updates

### Template Changes
```html:src/frontend/thrume-frontend/src/views/AccountPosts.vue
<template>
  <div class="profile-page">
    <ProfileHeader
      :username="profileData.userName"
      :pictureUrl="profileData.pictureUrl"
      :postCount="profileData.postCount"
      :followersCount="profileData.followersCount"
      :followingCount="profileData.followingCount"
    />

    <!-- Existing posts list -->
    <div v-if="processedPosts.length">
      <PostCard 
        v-for="post in processedPosts" 
        :key="post.id" 
        :post="post"
        @post-deleted="handlePostDeleted"
      />
    </div>
    <!-- ... rest of template ... -->
  </div>
</template>
```

### Script Changes
```typescript:src/frontend/thrume-frontend/src/views/AccountPosts.vue
// Add import
import ProfileHeader from '../components/ProfileHeader.vue';

// Add profile data ref
const profileData = ref({
  userName: '',
  pictureUrl: null,
  postCount: 0,
  followersCount: 0,
  followingCount: 0
});

// Add fetch function
const fetchProfileData = async (username: string) => {
  try {
    const response = await apiClient.get(`/account/profile/${username}`);
    profileData.value = response.data;
  } catch (error) {
    console.error('Error fetching profile data:', error);
  }
};

// Update onMounted and watch
onMounted(() => {
  // ...
  if (typeof currentUsernameParam === 'string') {
    username.value = currentUsernameParam;
    fetchProfileData(currentUsernameParam);
    fetchPostsByUsername(currentUsernameParam);
  }
  // ...
});

watch(
  () => route.params.name,
  async (newUsernameParam) => {
    if (typeof newUsernameParam === 'string' && newUsernameParam !== username.value) {
      username.value = newUsernameParam;
      await fetchProfileData(newUsernameParam);
      await fetchPostsByUsername(newUsernameParam);
    }
  }
);
```

## API Integration
- Endpoint: GET `/account/profile/{username}`
- Expected response format:
  ```json
  {
    "userName": "username",
    "pictureUrl": null,
    "postCount": 21,
    "followersCount": 0,
    "followingCount": 0
  }
  ```

## Validation Checklist
- [ ] Profile header displays correctly
- [ ] Instagram-style placeholder shows for null profile pictures
- [ ] Stats display correctly
- [ ] Profile data updates when username changes
- [ ] Error handling for profile data fetch
- [ ] Responsive design on mobile
- [ ] Cohesive styling with existing components

## Responsive Design
```css
@media (max-width: 768px) {
  .profile-header {
    flex-direction: column;
    text-align: center;
  }
  
  .profile-picture-container {
    margin-right: 0;
    margin-bottom: 20px;
  }
  
  .stats {
    justify-content: center;
  }
}