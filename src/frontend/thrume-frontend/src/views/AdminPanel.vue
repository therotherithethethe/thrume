<template>
  <div class="admin-panel">
    <!-- Unauthorized Access Message -->
    <div v-if="!isAdmin" class="unauthorized">
      <h2>Unauthorized Access</h2>
      <p>You do not have permission to view this page.</p>
    </div>

    <div v-else>
      <!-- Loading State -->
      <div v-if="loading" class="loading-container">
        <div class="loader">Loading...</div>
      </div>

      <div v-else>
        <!-- Error Message -->
        <div v-if="error" class="error-message">
          {{ error }}
        </div>

        <!-- Header: Title, Search, and Refresh -->
        <div class="header-container">
          <h1>User Management</h1>
          <div class="search-container">
            <input
              type="text"
              v-model="searchQuery"
              placeholder="Search by username or email"
              class="search-input"
            />
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" class="search-icon">
              <path fill-rule="evenodd" d="M9 3.5a5.5 5.5 0 100 11 5.5 5.5 0 000-11zM2 9a7 7 0 1112.452 4.391l3.328 3.329a.75.75 0 11-1.06 1.06l-3.329-3.328A7 7 0 012 9z" clip-rule="evenodd" />
            </svg>
          </div>
          <button @click="fetchUsers" class="refresh-button">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M4 2a1 1 0 011 1v2.101a7.002 7.002 0 0111.601 2.566 1 1 0 11-1.885.666A5.002 5.002 0 005.999 7H9a1 1 0 010 2H4a1 1 0 01-1-1V3a1 1 0 011-1zm.008 9.057a1 1 0 011.276.61A5.002 5.002 0 0014.001 13H11a1 1 0 110-2h5a1 1 0 011 1v5a1 1 0 11-2 0v-2.101a7.002 7.002 0 01-11.601-2.566 1 1 0 01.61-1.276z" clip-rule="evenodd" />
            </svg>
            Refresh
          </button>
        </div>

        <!-- Empty State (No Users Found) -->
        <div v-if="filteredUsers.length === 0" class="empty-state">
          <h3>No users found</h3>
          <p v-if="searchQuery">No users match your search for "{{ searchQuery }}"</p>
          <p v-else>There are no users to display at this time.</p>
        </div>

        <!-- Users Table -->
        <div v-else class="table-container">
          <table class="user-table">
            <thead>
              <tr>
                <th>User Name</th>
                <th>Profile Picture</th>
                <th>Email Confirmed</th>
                <th>Roles</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <template v-for="user in filteredUsers" :key="user.id">
                <!-- User Row -->
                <tr>
                  <td>
                    <div class="user-name">{{ user.userName }}</div>
                  </td>
                  <td>
                    <div class="avatar-container">
                      <img
                        v-if="user.pictureUrl"
                        :src="user.pictureUrl"
                        alt="Profile"
                        class="avatar"
                      >
                      <div v-else class="avatar-placeholder"></div>
                    </div>
                  </td>
                  <td>
                    <svg
                      v-if="user.isEmailConfirmed"
                      xmlns="http://www.w3.org/2000/svg"
                      class="icon confirmed"
                      viewBox="0 0 20 20"
                      fill="currentColor"
                    >
                      <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
                    </svg>
                    <svg
                      v-else
                      xmlns="http://www.w3.org/2000/svg"
                      class="icon not-confirmed"
                      viewBox="0 0 20 20"
                      fill="currentColor"
                    >
                      <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
                    </svg>
                  </td>
                  <td class="roles">
                    {{ user.roles.join(', ') }}
                  </td>
                  <td class="actions">
                    <button @click="startEditing(user)" class="edit-button" title="Edit user">
                      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" class="icon">
                        <path d="m5.433 13.917 1.262-3.155A4 4 0 0 1 7.58 9.42l6.92-6.918a2.121 2.121 0 0 1 3 3l-6.92 6.918c-.383.383-.84.685-1.343.886l-3.154 1.262a.5.5 0 0 1-.65-.65Z" />
                        <path d="M3.5 5.75c0-.69.56-1.25 1.25-1.25H10A.75.75 0 0 0 10 3H4.75A2.75 2.75 0 0 0 2 5.75v9.5A2.75 2.75 0 0 0 4.75 18h9.5A2.75 2.75 0 0 0 17 15.25V10a.75.75 0 0 0-1.5 0v5.25c0 .69-.56 1.25-1.25 1.25h-9.5c-.69 0-1.25-.56-1.25-1.25v-9.5Z" />
                      </svg>
                    </button>
                  </td>
                </tr>
                <!-- Editing Form (collapsible row) -->
                <tr v-if="editingUserId === user.id">
                  <td colspan="5">
                    <div class="edit-form-container">
                      <form @submit.prevent="updateUser(user.userName)" class="edit-form">
                        <div class="form-group">
                          <label for="role">Role</label>
                          <select id="role" v-model="form.role">
                            <option value="Admin">Admin</option>
                            <option value="User">User</option>
                            <option value="Banned">Banned</option>
                          </select>
                        </div>
                        <div class="form-group">
                          <label for="newNickname">New Nickname</label>
                          <input type="text" id="newNickname" v-model="form.newNickname" />
                        </div>
                        <div class="form-group">
                          <label for="email">Email</label>
                          <input type="email" id="email" v-model="form.email" />
                        </div>
                        <div class="form-group checkbox-group">
                          <label for="isNeedToConfirmEmail">
                            <input type="checkbox" id="isNeedToConfirmEmail" v-model="form.isNeedToConfirmEmail" />
                            Require Email Confirmation
                          </label>
                        </div>
                        <div v-if="formError" class="form-error">{{ formError }}</div>
                        <div class="form-actions">
                          <button type="submit" :disabled="updating">Save</button>
                          <button type="button" @click="editingUserId = null" :disabled="updating">Cancel</button>
                        </div>
                      </form>
                    </div>
                  </td>
                </tr>
              </template>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import axiosInstance from '../axiosInstance';
import { useAccountStore } from '../stores/accountStore';

const accountStore = useAccountStore();
const isAdmin = computed(() => accountStore.roles?.includes('Admin'));

interface User {
  id: string;
  userName: string;
  pictureUrl: string | null;
  isEmailConfirmed: boolean;
  roles: string[];
  email: string;
}

const users = ref<User[]>([]);
const searchQuery = ref('');
const loading = ref(true);
const error = ref<string | null>(null);
const editingUserId = ref<string | null>(null);
const updating = ref(false);
const formError = ref<string | null>(null);
const form = ref({
  role: '',
  newNickname: '',
  email: '',
  isNeedToConfirmEmail: true
});

const filteredUsers = computed(() => {
  if (!searchQuery.value) return users.value;
  
  const query = searchQuery.value.toLowerCase();
  return users.value.filter(user =>
    user.userName.toLowerCase().includes(query) ||
    (user.email && user.email.toLowerCase().includes(query))
  );
});

const fetchUsers = async () => {
  try {
    loading.value = true;
    error.value = null;
    const response = await axiosInstance.get('/api/admin/account');
    users.value = response.data.map((user: any) => ({
      ...user,
      email: user.email || '' // Ensure email property exists
    }));
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Failed to fetch users';
  } finally {
    loading.value = false;
  }
};

const startEditing = (user: User) => {
  editingUserId.value = user.id;
  form.value = {
    role: user.roles[0] || 'User',
    newNickname: user.userName,
    email: user.email || '',
    isNeedToConfirmEmail: false
  };
  formError.value = null;
};

const updateUser = async (userName: string) => {
  formError.value = null;
  const allowedRoles = ['Admin', 'User', 'Banned'];
  if (!allowedRoles.includes(form.value.role)) {
    formError.value = `Invalid role. Allowed roles: ${allowedRoles.join(', ')}`;
    return;
  }

  updating.value = true;
  try {
    await axiosInstance.put(`/api/admin/change/${userName}`, {
      role: form.value.role,
      newNickname: form.value.newNickname,
      email: form.value.email,
      isNeedToConfirmEmail: form.value.isNeedToConfirmEmail
    });
    await fetchUsers(); // Refresh data on success
    editingUserId.value = null; // Close form
  } catch (err: any) {
    formError.value = err.response?.data?.message || 'Failed to update user';
  } finally {
    updating.value = false;
  }
};

onMounted(() => {
  if (isAdmin.value) {
    fetchUsers();
  } else {
    loading.value = false;
  }
});
</script>

<style scoped>
.admin-panel {
  padding: 2rem;
  background-color: #f9fafb; /* Lighter gray background for the whole panel */
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
  min-height: 100vh;
}

/* --- UNAUTHORIZED BLOCK --- */
.unauthorized {
  text-align: center;
  padding: 40px;
  background-color: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
  max-width: 500px;
  margin: 40px auto;
}
.unauthorized h2 {
  color: #ef4444; /* red-500 */
  margin-bottom: 16px;
}
.unauthorized p {
  color: #6b7280; /* gray-500 */
  font-size: 16px;
}

/* --- STATES: LOADING, ERROR, EMPTY --- */
.loading-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 256px;
  font-size: 18px;
  color: #6b7280;
}
.error-message {
  background-color: #fee2e2;
  border: 1px solid #fecaca;
  color: #b91c1c;
  padding: 12px 16px;
  border-radius: 8px;
  margin-bottom: 20px;
}
.empty-state {
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
  padding: 48px 24px;
  text-align: center;
  border: 1px solid #e5e7eb;
}
.empty-state h3 {
  font-size: 18px;
  font-weight: 500;
  color: #111827;
  margin: 0 0 8px;
}
.empty-state p {
  color: #6b7280;
  margin: 0;
}

/* --- HEADER & SEARCH BAR --- */
.header-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  gap: 16px;
}
.header-container h1 {
  font-size: 28px;
  font-weight: bold;
  margin: 0;
  color: #111827;
}
.search-container {
  position: relative;
  flex-grow: 1;
  max-width: 400px;
}
.search-input {
  width: 100%;
  padding: 10px 16px 10px 40px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 14px;
  transition: border-color 0.2s, box-shadow 0.2s;
}
.search-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.3);
}
.search-icon {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  height: 20px;
  width: 20px;
  color: #9ca3af;
}
.refresh-button {
  background-color: #4f46e5; /* Indigo */
  color: white;
  font-weight: 600;
  padding: 10px 16px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: background-color 0.2s;
}
.refresh-button:hover {
  background-color: #4338ca;
}
.refresh-button svg {
  height: 20px;
  width: 20px;
}

/* --- TABLE STYLES --- */
.table-container {
  overflow-x: auto;
  background-color: #ffffff;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
  border: 1px solid #e5e7eb;
}
.user-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 800px;
}
.user-table thead {
  background-color: #f9fafb;
}
.user-table th {
  text-align: left;
  padding: 12px 24px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #6b7280;
}
.user-table tbody tr {
  border-bottom: 1px solid #e5e7eb;
}
.user-table tbody tr:last-child {
  border-bottom: none;
}
.user-table tbody tr:hover {
  background-color: #f9fafb;
}
.user-table td {
  padding: 16px 24px;
  vertical-align: middle;
}
.user-name {
  font-weight: 500;
  color: #111827;
}
.avatar-container {
  display: flex;
}
.avatar {
  height: 40px;
  width: 40px;
  border-radius: 50%;
  object-fit: cover;
}
.avatar-placeholder {
  background-color: #e5e7eb;
  border-radius: 50%;
  width: 40px;
  height: 40px;
}
.icon {
  height: 24px;
  width: 24px;
}
.icon.confirmed { color: #10b981; }
.icon.not-confirmed { color: #ef4444; }
.roles {
  font-size: 14px;
  color: #6b7280;
}
.actions {
  text-align: center;
}
.edit-button {
  background-color: transparent;
  border: none;
  cursor: pointer;
  padding: 6px;
  border-radius: 50%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  color: #6b7280;
  transition: background-color 0.2s, color 0.2s;
}
.edit-button .icon {
  height: 20px;
  width: 20px;
}
.edit-button:hover {
  background-color: #e5e7eb;
  color: #111827;
}

/* --- EDIT FORM STYLES --- */
.edit-form-container {
  padding: 24px;
  background-color: #f9fafb;
}
.edit-form {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px 24px;
}
.form-group {
  display: flex;
  flex-direction: column;
}
.checkbox-group label {
  display: flex;
  align-items: center;
  cursor: pointer;
}
.form-group label {
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 4px;
  color: #374151;
}
.form-group input[type="text"],
.form-group input[type="email"],
.form-group select {
  padding: 8px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  background-color: #fff;
}
.form-group input:focus, .form-group select:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 2px rgba(79, 70, 229, 0.3);
}
.form-group input[type="checkbox"] {
  height: 16px;
  width: 16px;
  margin-right: 8px;
}
.form-actions {
  grid-column: span 2;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding-top: 16px;
  border-top: 1px solid #e5e7eb;
  margin-top: 8px;
}
.form-actions button {
  padding: 8px 16px;
  border: 1px solid transparent;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}
.form-actions button[type="submit"] {
  background-color: #4f46e5;
  color: white;
}
.form-actions button[type="submit"]:hover {
  background-color: #4338ca;
}
.form-actions button[type="button"] {
  background-color: #fff;
  color: #374151;
  border-color: #d1d5db;
}
.form-actions button[type="button"]:hover {
  background-color: #f9fafb;
  border-color: #9ca3af;
}
.form-error {
  grid-column: span 2;
  background-color: #fee2e2;
  color: #b91c1c;
  padding: 10px 16px;
  border-radius: 6px;
  font-size: 14px;
  margin-top: 8px;
}

/* Disabled state for buttons */
button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.form-actions button:disabled:hover {
  background-color: #4f46e5; /* Keep original color */
  border-color: transparent;
}
.form-actions button[type="button"]:disabled:hover {
  background-color: #fff;
  border-color: #d1d5db;
}
</style>