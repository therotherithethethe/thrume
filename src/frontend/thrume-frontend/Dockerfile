# Етап 1: Використовуємо офіційний образ Nginx
FROM nginx:stable-alpine

# Видаляємо стандартну конфігурацію Nginx
RUN rm /etc/nginx/conf.d/default.conf

# Копіюємо наш власний файл конфігурації
# Припускаємо, що nginx.conf лежить поруч з Dockerfile
COPY nginx.conf /etc/nginx/conf.d/

# Копіюємо зібрані файли нашого Vue-додатку
# в папку, яку ми вказали як 'root' у nginx.conf
COPY dist/ /usr/share/nginx/html

# Відкриваємо порт, який ми слухаємо у nginx.conf
EXPOSE 80

# Команда для запуску Nginx
CMD ["nginx", "-g", "daemon off;"]