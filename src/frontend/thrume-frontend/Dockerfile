# Файл: Dockerfile
# Розташування: ./src/frontend/thrume-frontend/Dockerfile

# --- ЕТАП 1: Збірка (Build Stage) ---
# Використовуємо офіційний образ Node.js для збірки нашого Vue-проєкту
FROM node:20-alpine AS build

# Встановлюємо pnpm глобально
RUN npm install -g pnpm

# Встановлюємо робочу директорію всередині контейнера
WORKDIR /app

# Копіюємо package.json та pnpm-lock.yaml для кешування залежностей
COPY package.json pnpm-lock.yaml ./

# Встановлюємо залежності
RUN pnpm install

# Копіюємо решту файлів проєкту
COPY . .

# Збираємо проєкт. Результат буде в папці /app/dist
RUN pnpm run build

# --- ЕТАП 2: Фінальний образ (Final Stage) ---
# Використовуємо легкий образ Nginx для запуску
FROM nginx:stable-alpine

# Копіюємо наш власний nginx.conf
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Копіюємо зібрані файли з попереднього етапу (з папки /app/dist)
# в директорію, яку обслуговує Nginx
COPY --from=build /app/dist /usr/share/nginx/html

# Відкриваємо порт 80
EXPOSE 80

# Команда для запуску Nginx
CMD ["nginx", "-g", "daemon off;"]