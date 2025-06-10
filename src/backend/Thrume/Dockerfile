# --- Етап 1: Збірка (Build Stage) ---
FROM mcr.microsoft.com/dotnet/sdk:9.0.200 AS build
WORKDIR /src

# --- ВАЖЛИВІ ЗМІНИ ТУТ ---
# Спочатку копіюємо .sln та ВСІ .csproj файли, які потрібні для restore.
# Це оптимізує кешування Docker: залежності не будуть завантажуватись знову,
# якщо ви змінили тільки код, а не файли проєктів.

COPY Thrume.sln .
COPY Thrume.Api/Thrume.Api.csproj Thrume.Api/
COPY Thrume.Database/Thrume.Database.csproj Thrume.Database/
COPY Thrume.Domain/Thrume.Domain.csproj Thrume.Domain/
COPY Thrume.Common/Thrume.Common.csproj Thrume.Common/
COPY Thrume.Configuration/Thrume.Configuration.csproj Thrume.Configuration/
COPY Thrume.Infrastructure/Thrume.Infrastructure.csproj Thrume.Infrastructure/
COPY Thrume.Services/Thrume.Services.csproj Thrume.Services/

# Тепер restore знайде всі проєкти, на які посилається .sln файл.
RUN dotnet restore "Thrume.sln"

# Після відновлення залежностей копіюємо решту файлів проєкту.
COPY . .
WORKDIR "/src/Thrume.Api"

# Публікуємо головний проєкт.
# --no-restore, оскільки ми вже виконали restore.
RUN dotnet publish "Thrume.Api.csproj" -c Release -o /app/publish --no-build

# --- Етап 2: Фінальний образ (Final Stage) ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0.2 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Thrume.Api.dll"]