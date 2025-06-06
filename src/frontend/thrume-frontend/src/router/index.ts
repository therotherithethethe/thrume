import { createMemoryHistory, createRouter, createWebHistory, RouteRecordRaw } from "vue-router"
import Home from "../views/Home.vue"
import Auth from "../views/Auth.vue"
import AccountPosts from "../views/AccountPosts.vue"
import MessagesView from "../views/MessagesView.vue"
import ConversationView from "../views/ConversationView.vue"

const routes: Array<RouteRecordRaw> = [
    {
        path: '/',
        name: 'Home',
        component: Home
    },
    {
        path: '/auth/login',
        name: 'Auth',
        component: Auth
    },
    {
        path: '/messages',
        name: 'Messages',
        component: MessagesView,
        meta: { requiresAuth: true }
    },
    {
        path: '/messages/:conversationId',
        name: 'Conversation',
        component: ConversationView,
        meta: { requiresAuth: true }
    },
    {
        path: '/:name',
        component: AccountPosts
    }
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

export default router