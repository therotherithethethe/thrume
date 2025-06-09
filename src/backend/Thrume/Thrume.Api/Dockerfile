# --- Етап 1: Збірка (Build Stage) ---
# Використовуємо повний SDK для компіляції
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# !!! ЗЛАМУЄМО КЕШ !!!
# Кожного разу, коли потрібна повна перебудова, змініть це значення
ARG CACHE_BUSTER=1 

WORKDIR /src

# --- ДІАГНОСТИКА ---
# Показуємо вміст поточної директорії (контексту збірки)
RUN ls -la
# ------------------

# Копіюємо .sln та всі файли .csproj...
COPY ["*.sln", "Directory.Packages.props", "./"]
COPY **/*.csproj ./

# ... решта файлу ...