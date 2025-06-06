import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'
import fs from 'fs'


// https://vite.dev/config/
export default defineConfig({
  server: {
    // @ts-ignore
    https: {
      key: fs.readFileSync('localhost-key.pem'),
      cert: fs.readFileSync('localhost.pem')
    },
    proxy: {
      '/chathub': {
        target: 'https://localhost:5133',
        changeOrigin: true,
        secure: false,
        ws: true, // Enable WebSocket proxying for SignalR
      },
      '/': {
        target: 'https://localhost:5133',
        changeOrigin: true,
        secure: false,
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
