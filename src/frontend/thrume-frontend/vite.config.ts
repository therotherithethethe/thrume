import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'


// https://vite.dev/config/
export default defineConfig({
  server: {
    // @ts-ignore
    //https: true,
    proxy: {
      '/auth/login': {
        target: 'https://localhost:5133',
        changeOrigin: true,
        secure: false
      },
      '/auth/register': {
        target: 'https://localhost:5133',
        changeOrigin: true,
        secure: false
      },
      '/auth/logout': {
        target: 'https://localhost:5133',
        changeOrigin: true,
        secure: false
      },
      '/auth/manage/info': {
        target: 'https://localhost:5133',
        changeOrigin: true,
        secure: false
      },
      '/posts': {
        target: 'https://localhost:5133',
        changeOrigin: true,
        secure: false
      },
      '/antiforgery': {
        target: 'https://localhost:5133',
        changeOrigin: true,
        secure: false
      }
    }
  },
  plugins: [
    vue(),
    vueJsx(),
    //vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  }
})
