# Profile Page Implementation Plan

## Objective
Implement a feature for the `/:name` route that displays:
- Account username prominently
- Profile picture in a circular format (show Instagram-style placeholder if `pictureUrl` is null)
- Followers count and following count clearly labeled
- All user posts matching the previous implementation's style

## Backend Requirements
1. Create new endpoint: `GET /account/profile/{userName}`
2. Return JSON structure:
```json
{
  "userName": "c.lohinov.yevhen@student.uzhnu.edu.ua",
  "pictureUrl": null,
  "postCount": 21,
  "followersCount": 0,
  "followingCount": 0
}
```

## Frontend Implementation

### File: `src/views/AccountPosts.vue`
1. Add new state variables:
```typescript
const profileData = ref<ProfileData | null>(null)
const profileLoading = ref(true)
const profileError = ref<string | null>(null)
```

2. Create profile data fetch function:
```typescript
const fetchProfileData = async (name: string) => {
  profileLoading.value = true
  try {
    const response = await apiClient.get(`/account/profile/${name}`)
    profileData.value = response.data
  } catch (err) {
    profileError.value = 'Failed to load profile data'
  } finally {
    profileLoading.value = false
  }
}
```

3. Call `fetchProfileData` in `onMounted` and route watcher

4. Add new ProfileHeader component:
```html
<ProfileHeader 
  v-if="profileData"
  :user-name="profileData.userName"
  :picture-url="profileData.pictureUrl"
  :followers="profileData.followersCount"
  :following="profileData.followingCount"
/>
```

### New Component: `src/components/ProfileHeader.vue`
```html
<template>
  <div class="profile-header">
    <div class="profile-picture">
      <img v-if="pictureUrl" :src="pictureUrl" class="circular-image">
      <div v-else class="placeholder circular-image">
        <svg width="60" height="60" viewBox="0 0 24 24">
          <circle cx="12" cy="12" r="10" fill="#e0e0e0"/>
          <path d="M12 12m-4 0a4 4 0 1 0 8 0a4 4 0 1 0 -8 0" fill="#9e9e9e"/>
        </svg>
      </div>
    </div>
    
    <div class="profile-info">
      <h1>{{ userName }}</h1>
      <div class="stats">
        <div class="stat">
          <span class="count">{{ postCount }}</span>
          <span class="label">posts</span>
        </div>
        <div class="stat">
          <span class="count">{{ followers }}</span>
          <span class="label">followers</span>
        </div>
        <div class="stat">
          <span class="count">{{ following }}</span>
          <span class="label">following</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps({
  userName: String,
  pictureUrl: String,
  followers: Number,
  following: Number,
  postCount: Number
})
</script>

<style scoped>
.profile-header {
  display: flex;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #e0e0e0;
  margin-bottom: 20px;
}

.circular-image {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid #fff;
  box-shadow: 0 2px 6px rgba(0,0,0,0.1);
}

.placeholder {
  display: flex;
  justify-content: center;
  align-items: center;
  background: #f5f5f5;
}

.profile-info {
  margin-left: 30px;
}

h1 {
  margin: 0 0 10px 0;
  font-size: 28px;
}

.stats {
  display: flex;
  gap: 20px;
}

.stat {
  text-align: center;
}

.count {
  display: block;
  font-weight: bold;
  font-size: 18px;
}

.label {
  font-size: 14px;
  color: #666;
}
</style>
```

### Type Definitions
Add to `src/types/index.ts`:
```typescript
export interface ProfileData {
  userName: string;
  pictureUrl: string | null;
  postCount: number;
  followersCount: number;
  followingCount: number;
}
```

## Testing Plan
1. Verify all elements render correctly
2. Test with null profile picture
3. Test with various follower/following counts
4. Verify responsive layout