import { createMemoryHistory, createRouter, createWebHistory, RouteRecordRaw } from "vue-router"
import Home from "../views/Home.vue"
import Auth from "../views/Auth.vue"
import AccountPosts from "../views/AccountPosts.vue"
import MessagesView from "../views/MessagesView.vue"
import ConversationViewEnhanced from "../views/ConversationViewEnhanced.vue"
import AccountSearch from "../views/AccountSearch.vue"
import Register from "../views/Register.vue"

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
        path: '/auth/register',
        name: 'Register',
        component: Register
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
        component: ConversationViewEnhanced,
        meta: { requiresAuth: true }
    },
    {
        path: '/:name',
        component: AccountPosts
    },
    {
        path: '/search/:name',
        component: AccountSearch
    }
    
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

export default router