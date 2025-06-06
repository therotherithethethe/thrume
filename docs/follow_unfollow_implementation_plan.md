# Follow/Unfollow Implementation Plan

## Objective
Implement a toggle button component for user profiles that:
- Shows "Edit Profile" when viewing own profile
- Shows "Follow" when not following another user
- Shows "Unfollow" when already following another user
- Updates followers count in real-time
- Uses existing POST endpoints for follow/unfollow actions

## Backend Requirements
### 1. Update Profile Endpoint
File: `src/backend/Thrume/Thrume.Api/Endpoints/AccountEndpoints.cs`
- Add `amIFollowing` field to response
- Add `id` field (target account ID) for API calls
- Include authentication context check

```cs
accountGroup.MapGet("/profile/{userName}", async (HttpContext context, AppDbContext dbContext, string userName) => {
    var findAsync = await dbContext.AccountDbSet
        .Where(a => a.UserName == userName)
        .Select(a => new {
            a.Id, // Add this line
            a.UserName,
            a.PictureUrl,
            PostCount = a.Posts.Count,
            FollowersCount = a.Followers.Count,
            FollowingCount = a.Following.Count,
        })
        .FirstOrDefaultAsync();
        
    if (findAsync is null) return Results.NotFound($"User {userName} not found");

    bool amIFollowing = false;
    var currentUserId = GetCurrentUserId(context.User);
    if (currentUserId != null) {
        amIFollowing = await dbContext.Subscriptions
            .AnyAsync(s => s.FollowerId == currentUserId && s.FollowingId == findAsync.Id);
    }

    return Results.Ok(new {
        ...findAsync,
        amIFollowing
    });
}).AllowAnonymous();
```

## Frontend Implementation

### 1. Update Type Definitions
File: `src/frontend/thrume-frontend/src/types/index.ts`
```ts
export interface ProfileData {
  userName: string;
  pictureUrl: string | null;
  postCount: number;
  followersCount: number;
  followingCount: number;
  amIFollowing: boolean; // Add this
  id: string; // Add account ID
}
```

### 2. Create FollowButton Component
File: `src/frontend/thrume-frontend/src/components/FollowButton.vue`
```vue
<template>
  <button :disabled="loading" @click="handleClick" :class="buttonClasses">
    <span v-if="loading">...</span>
    <span v-else>{{ buttonText }}</span>
  </button>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import apiClient from '../axiosInstance';

const props = defineProps({
  isOwnProfile: Boolean,
  isFollowing: Boolean,
  targetAccountId: String
});

const emit = defineEmits(['follow-status-changed']);

const loading = ref(false);
const error = ref<string | null>(null);

const buttonText = computed(() => {
  if (props.isOwnProfile) return 'Edit Profile';
  return props.isFollowing ? 'Unfollow' : 'Follow';
});

const buttonClasses = computed(() => ({
  'follow-button': true,
  'own-profile': props.isOwnProfile,
  following: props.isFollowing && !props.isOwnProfile,
  loading: loading.value
}));

const handleClick = async () => {
  if (props.isOwnProfile) {
    // Handle edit profile
    return;
  }

  loading.value = true;
  try {
    const endpoint = props.isFollowing 
      ? `/account/unfollow/${props.targetAccountId}`
      : `/account/follow/${props.targetAccountId}`;
      
    await apiClient.post(endpoint);
    emit('follow-status-changed', !props.isFollowing);
  } catch (err) {
    error.value = props.isFollowing 
      ? 'Failed to unfollow' 
      : 'Failed to follow';
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
/* Add styling here */
</style>
```

### 3. Update ProfileHeader Component
File: `src/frontend/thrume-frontend/src/components/ProfileHeader.vue`
```vue
<template>
  <div class="profile-header">
    <!-- ... existing content ... -->
    <FollowButton
      :is-own-profile="isOwnProfile"
      :is-following="amIFollowing"
      :target-account-id="targetAccountId"
      @follow-status-changed="handleFollowStatusChanged"
    />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useAuthStore } from '../stores/authStore';
import FollowButton from './FollowButton.vue';

const authStore = useAuthStore();
const props = defineProps({
  userName: String,
  pictureUrl: String,
  followers: Number,
  following: Number,
  postCount: Number,
  amIFollowing: Boolean, // Add this
  targetAccountId: String // Add this
});

const emit = defineEmits(['update-followers']);

const isOwnProfile = computed(() => {
  return authStore.account?.username === props.userName;
});

const handleFollowStatusChanged = (newStatus: boolean) => {
  emit('update-followers', newStatus ? 1 : -1);
};
</script>
```

### 4. Update AccountPosts.vue
File: `src/frontend/thrume-frontend/src/views/AccountPosts.vue`
- Add targetAccountId to profileData
- Add update-followers handler
- Pass required props to ProfileHeader

## Testing Plan
1. Verify button shows correct state (Edit/Follow/Unfollow)
2. Test follow/unfollow API calls
3. Verify real-time followers count update
4. Test error handling
5. Verify own profile detection