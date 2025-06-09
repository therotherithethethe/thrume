# --- Етап 1: Збірка (Build Stage) ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# --- ДІАГНОСТИКА ---
# Показуємо вміст поточної директорії (контексту збірки)
RUN ls -la
# ------------------

# Копіюємо .sln та всі файли .csproj...
COPY ["*.sln", "Directory.Packages.props", "./"]
COPY **/*.csproj ./

# ... решта файлу ...