    # --- Етап 1: Збірка (Build Stage) ---
    FROM mcr.microsoft.com/dotnet/sdk:9.0.200 AS build

    # Копіюємо весь проєкт одразу, зберігаючи структуру папок
    COPY . /src
    WORKDIR /src

    # Тепер restore знайде всі файли, тому що структура папок правильна
    RUN dotnet restore "Thrume.sln"

    # Публікуємо проєкт
    # --no-restore, бо ми вже зробили restore на попередньому кроці
    RUN dotnet publish "Thrume.Api/Thrume.Api.csproj" -c Release -o /app/publish --no-restore

    # --- Етап 2: Фінальний образ (Final Stage) ---
    FROM mcr.microsoft.com/dotnet/aspnet:9.0.2 AS final
    WORKDIR /app
    COPY --from=build /app/publish .
    ENV ASPNETCORE_URLS=http://+:8080
    EXPOSE 8080
    ENTRYPOINT ["dotnet", "Thrume.Api.dll"]