# --- Етап 1: Збірка (Build Stage) ---
# Використовуємо повний SDK для компіляції
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копіюємо .sln та всі файли .csproj, зберігаючи структуру директорій.
# Це оптимізує кешування шару відновлення.
COPY ["*.sln", "Directory.Packages.props", "./"]
# `**` знаходить файли в піддиректоріях, а копіювання зберігає їхню структуру.
COPY **/*.csproj ./ 

# Відновлюємо пакети для всього рішення
RUN dotnet restore "Thrume.sln"

# Копіюємо решту файлів проєкту
COPY . .

# Публікуємо проєкт. Команда publish автоматично виконує build.
# Результат зберігається в /app/publish
# --no-restore використовується, оскільки ми вже відновили пакети на попередньому кроці
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