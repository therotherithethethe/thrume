# === ЕТАП 1: Збірка (Builder) ===
# Використовуємо офіційний образ Node.js для збірки проєкту.
# Версія 'alpine' легша, що прискорює процес.
FROM node:18-alpine AS builder

# Встановлюємо робочу директорію всередині контейнера.
WORKDIR /app

# Копіюємо package.json та package-lock.json для кешування залежностей.
# Docker не буде перевстановлювати їх, якщо ці файли не змінилися.
COPY package*.json ./

# Встановлюємо залежності.
RUN npm install

# Копіюємо всі інші файли проєкту.
COPY . .

# Запускаємо команду збірки, яка створить оптимізовані файли в папці /dist.
RUN npm run build


# === ЕТАП 2: Production (Nginx) ===
# Використовуємо легкий та швидкий веб-сервер Nginx.
FROM nginx:stable-alpine

# Видаляємо стандартну конфігурацію Nginx.
RUN rm /etc/nginx/conf.d/default.conf

# Копіюємо нашу власну конфігурацію Nginx.
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Копіюємо зібрані статичні файли з етапу 'builder' в директорію,
# з якої Nginx буде їх роздавати.
COPY --from=builder /app/dist /usr/share/nginx/html

# Повідомляємо Docker, що контейнер буде слухати порт 80.
# Dokploy та інші системи оркестрації використовують цю інформацію.
EXPOSE 80

# Команда для запуску Nginx.
CMD ["nginx", "-g", "daemon off;"]