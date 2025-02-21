# Build stage: Use the .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all csproj files and restore dependencies
COPY Thrume.Api/Thrume.Api.csproj Thrume.Api/
COPY Thrume.Database/Thrume.Database.csproj Thrume.Database/
COPY Thrume.Domain/Thrume.Domain.csproj Thrume.Domain/
RUN dotnet restore Thrume.Api/Thrume.Api.csproj

# Copy the entire solution and build
COPY . .
WORKDIR /src/Thrume.Api
RUN dotnet publish -c Release -o /app

# Runtime stage: Use the ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Expose the port
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "Thrume.Api.dll"]