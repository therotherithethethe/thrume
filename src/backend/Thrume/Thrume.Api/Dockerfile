FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Thrume.Api/Thrume.Api.csproj", "Thrume.Api/"]
COPY ["Thrume.Common/Thrume.Common.csproj", "Thrume.Common/"]
COPY ["Thrume.Domain/Thrume.Domain.csproj", "Thrume.Domain/"]
COPY ["Thrume.Configuration/Thrume.Configuration.csproj", "Thrume.Configuration/"]
COPY ["Thrume.Database/Thrume.Database.csproj", "Thrume.Database/"]
COPY ["Thrume.Services/Thrume.Services.csproj", "Thrume.Services/"]
COPY ["Thrume.Infrastructure/Thrume.Infrastructure.csproj", "Thrume.Infrastructure/"]
RUN dotnet restore "Thrume.Api/Thrume.Api.csproj"
COPY . .
WORKDIR "/src/Thrume.Api"
RUN dotnet build "Thrume.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Thrume.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Thrume.Api.dll"]
