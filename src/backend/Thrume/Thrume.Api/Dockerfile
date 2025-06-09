FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копіюємо файли з папки Thrume
COPY ["Thrume/*.sln", "Thrume/Directory.Packages.props", "./"]
COPY Thrume/**/*.csproj ./

# Переконуємось, що restore знайде проєкти
# (можливо доведеться вказати повний шлях до .sln)
RUN dotnet restore "./Thrume.sln"

# Копіюємо весь проєкт
COPY Thrume/. .

RUN dotnet publish "Thrume.Api/Thrume.Api.csproj" -c Release -o /app/publish --no-restore

# --- Етап 2: Фінальний образ (Final Stage) ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Thrume.Api.dll"]