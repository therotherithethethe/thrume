import Auth from "@/components/Auth.vue";
import Home from "@/views/Home.vue"; // Import the new Home view
import UserProfile from "@/views/UserProfile.vue"; // Import the UserProfile view
import ExploreView from "@/views/ExploreView.vue"; // Import the Explore view
// Remove unused Mom import if not needed elsewhere: import Mom from "@/components/Mom.vue";
import { createRouter, createWebHistory, type RouteRecordRaw } from "vue-router"; // Removed unused createMemoryHistory
import { useAuthStore } from "@/stores/auth";

const routes: Array<RouteRecordRaw> = [
    {
        path: '/',
        name: 'Home', // Add a name for the route
        component: Home
    },
    {
        path: '/auth',
        name: 'Auth', // Add a name for the route
        component: Auth,
        meta: { guestOnly: true }
    },
    {
        path: '/explore', // Add route for Explore page
        name: 'Explore',
        component: ExploreView
    },
    {
        path: '/:username', // Dynamic route for user profiles
        name: 'UserProfile',
        component: UserProfile,
        props: true, // Pass route params as props to the component
        meta: { requiresAuth: true }
    },
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

router.beforeEach(async (to, from, next) => {
    const authStore = useAuthStore();
    if (!authStore.isAuthenticated) {
        await authStore.initializeAuth();
    }
    if (to.meta.requiresAuth && !authStore.isAuthenticated) {
        next({ name: 'Auth', query: { redirect: to.fullPath } });
    } else if (to.meta.guestOnly && authStore.isAuthenticated) {
        next({ path: '/' });
    } else {
        next();
    }
});

export default router;