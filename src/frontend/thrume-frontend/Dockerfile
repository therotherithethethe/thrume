# Файл: Dockerfile
# Розташування: ./src/frontend/thrume-frontend/Dockerfile
# ВАЖЛИВО: Цей Dockerfile очікує, що збірка запускається з КОРЕНЯ репозиторію.

# --- ЕТАП 1: Збірка (Build Stage) ---
FROM node:20-alpine AS build

RUN npm install -g pnpm
WORKDIR /app

# --- КЛЮЧОВА ЗМІНА ---
# Копіюємо файли залежностей, вказуючи повний шлях від кореня репозиторію.
COPY src/frontend/thrume-frontend/package.json src/frontend/thrume-frontend/pnpm-lock.yaml ./

# Встановлюємо залежності
RUN pnpm install

# --- КЛЮЧОВА ЗМІНА ---
# Копіюємо весь вміст папки фронтенду в робочу директорію контейнера.
COPY src/frontend/thrume-frontend/ .

# Збираємо проєкт.
RUN pnpm run build

# --- ЕТАП 2: Фінальний образ (Final Stage) ---
FROM nginx:stable-alpine

# --- КЛЮЧОВА ЗМІНА ---
# Копіюємо конфігурацію Nginx, вказуючи повний шлях від кореня репозиторію.
COPY src/frontend/thrume-frontend/nginx.conf /etc/nginx/conf.d/default.conf

# Копіюємо зібрані файли з попереднього етапу. Ця команда не змінюється,
# бо вона копіює з іншого етапу всередині Docker, а не з хоста.
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]