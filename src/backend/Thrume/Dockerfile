# --- Етап 1: Збірка (Build Stage) ---
# Цей Dockerfile очікує, що збірка запускається з КОРЕНЯ репозиторію.
FROM mcr.microsoft.com/dotnet/sdk:9.0.200 AS build
WORKDIR /src

# --- КЛЮЧОВА ЗМІНА ---
# Копіюємо вміст папки /src/backend/Thrume/ з хоста (контексту збірки)
# в поточну робочу папку /src всередині контейнера.
# Крапка в кінці важлива!
COPY src/backend/Thrume/ .

# Після цього кроку структура файлів всередині /src контейнера буде правильною.
# /src/Thrume.sln
# /src/Thrume.Api/
# ... і так далі

# Тепер restore і publish працюватимуть без змін.
RUN dotnet restore "Thrume.sln"
RUN dotnet publish "Thrume.Api/Thrume.Api.csproj" -c Release -o /app/publish --no-restore

# --- Етап 2: Фінальний образ (Final Stage) ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0.2 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Thrume.Api.dll"]