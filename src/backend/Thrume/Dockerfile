# --- Етап 1: Збірка (Build Stage) ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Шляхи вказані без префіксів, бо Build Context = src/backend/Thrume
COPY ["*.sln", "Directory.Packages.props", "./"]
COPY **/*.csproj ./

RUN dotnet restore "Thrume.sln"

COPY . .

RUN dotnet publish "Thrume.Api/Thrume.Api.csproj" -c Release -o /app/publish --no-restore

# --- Етап 2: Фінальний образ (Final Stage) ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Thrume.Api.dll"]