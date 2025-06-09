# --- Етап 1: Збірка (Build Stage) ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Копіюємо весь проєкт одразу, зберігаючи структуру папок
COPY . /src
WORKDIR /src

# Виконуємо publish. Ця команда автоматично виконає restore і build.
# Це може бути більш ефективним з точки зору пам'яті.
RUN dotnet publish "Thrume.Api/Thrume.Api.csproj" -c Release -o /app/publish

# --- Етап 2: Фінальний образ (Final Stage) ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Thrume.Api.dll"]