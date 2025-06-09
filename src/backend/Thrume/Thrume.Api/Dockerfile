# --- Етап 1: Збірка ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Всі шляхи тепер починаються з "Thrume/"
COPY ["Thrume/Thrume.sln", "Thrume/Directory.Packages.props", "./"]
COPY Thrume/**/*.csproj ./

RUN dotnet restore "Thrume.sln"

COPY Thrume/. .

RUN dotnet publish "Thrume.Api/Thrume.Api.csproj" -c Release -o /app/publish --no-restore

# ... решта файлу без змін ...