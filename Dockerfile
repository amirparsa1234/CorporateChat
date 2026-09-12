# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project files first to improve Docker layer caching
COPY Server/Server.csproj Server/
COPY Shared/Shared.csproj Shared/

# Restore dependencies
RUN dotnet restore Server/Server.csproj

# Copy source code
COPY Server/ Server/
COPY Shared/ Shared/

# Publish the application
WORKDIR /src/Server

RUN dotnet publish Server.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    --property:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Create persistent directories
RUN mkdir -p /app/data/uploads \
    && chown -R $APP_UID:$APP_UID /app

# Run as a non-root user
USER $APP_UID

COPY --from=build /app/publish .

EXPOSE 8080

VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "Server.dll"]
