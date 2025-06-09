# --- Етап 1: Збірка (Build Stage) ---
# Використовуємо повний SDK для компіляції
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копіюємо .sln та всі .csproj файли для оптимізації кешування шару відновлення
COPY ["Thrume.sln", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["Thrume.Api/Thrume.Api.csproj", "Thrume.Api/"]
COPY ["Thrume.Common/Thrume.Common.csproj", "Thrume.Common/"]
COPY ["Thrume.Configuration/Thrume.Configuration.csproj", "Thrume.Configuration/"]
COPY ["Thrume.Database/Thrume.Database.csproj", "Thrume.Database/"]
COPY ["Thrume.Domain/Thrume.Domain.csproj", "Thrume.Domain/"]
COPY ["Thrume.Infrastructure/Thrume.Infrastructure.csproj", "Thrume.Infrastructure/"]


# Відновлюємо пакети для всього рішення
RUN dotnet restore "Thrume.sln"

# Копіюємо решту файлів проєкту
COPY . .

# Публікуємо проєкт. Команда publish автоматично виконує build.
# Результат зберігається в /app/publish
RUN dotnet publish "Thrume.Api/Thrume.Api.csproj" -c Release -o /app/publish --no-restore

# --- Етап 2: Фінальний образ (Final Stage) ---
# Використовуємо легкий образ ASP.NET Runtime для запуску
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS final
WORKDIR /app

# Копіюємо тільки скомпільований додаток з етапу "build"
COPY --from=build /app/publish .

# Налаштовуємо порт для роботи в контейнері
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Команда для запуску додатку
ENTRYPOINT ["dotnet", "Thrume.Api.dll"]