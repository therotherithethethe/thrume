# Файл: Dockerfile
# Призначення: Створити образ Nginx для обслуговування Vue-додатку

# Етап 1: Використовуємо стабільний і легкий офіційний образ Nginx
FROM nginx:stable-alpine

# Встановлюємо пакет ca-certificates.
# Це дозволяє Nginx довіряти SSL-сертифікатам при вихідних з'єднаннях (про всяк випадок).
RUN apk --no-cache add ca-certificates

# Видаляємо стандартну конфігурацію Nginx, щоб уникнути конфліктів.
RUN rm /etc/nginx/conf.d/default.conf

# Копіюємо наш власний файл конфігурації в потрібне місце всередині образу.
COPY nginx.conf /etc/nginx/conf.d/

# Копіюємо зібрані файли нашого Vue-додатку (з папки dist)
# в стандартну директорію для веб-контенту Nginx.
COPY dist/ /usr/share/nginx/html

# Повідомляємо Docker, що контейнер буде слухати на порту 80.
EXPOSE 80

# Команда для запуску Nginx у foreground-режимі, що є стандартом для контейнерів.
CMD ["nginx", "-g", "daemon off;"]