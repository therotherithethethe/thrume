# --- Етап 1: Збірка (Build Stage) ---
FROM mcr.microsoft.com/dotnet/sdk:9.0.200 AS build
WORKDIR /src

# Копіюємо .sln та .csproj файли для відновлення залежностей
# Це дозволить кешувати шар з залежностями
COPY Thrume.sln .
COPY Thrume.Api/Thrume.Api.csproj Thrume.Api/
# Якщо є інші проєкти, їх .csproj теж треба скопіювати
# COPY Thrume.Core/Thrume.Core.csproj Thrume.Core/
# ...

# Відновлюємо залежності
RUN dotnet restore "Thrume.sln"

# Тепер копіюємо решту файлів проєкту
COPY . .
WORKDIR "/src/Thrume.Api"

# Публікуємо проєкт
# --no-restore, бо ми вже зробили restore
RUN dotnet publish "Thrume.Api.csproj" -c Release -o /app/publish --no-build

# --- Етап 2: Фінальний образ (Final Stage) ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0.2 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Thrume.Api.dll"]