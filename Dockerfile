# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution structure and dependency manifests for Docker layer caching
COPY ["CleanArchitectureMediatorShowcase.slnx", "./"]
COPY ["global.json", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/CleanArchitectureMediatorShowcase.Domain/CleanArchitectureMediatorShowcase.Domain.csproj", "src/CleanArchitectureMediatorShowcase.Domain/"]
COPY ["src/CleanArchitectureMediatorShowcase.Application/CleanArchitectureMediatorShowcase.Application.csproj", "src/CleanArchitectureMediatorShowcase.Application/"]
COPY ["src/CleanArchitectureMediatorShowcase.Infrastructure/CleanArchitectureMediatorShowcase.Infrastructure.csproj", "src/CleanArchitectureMediatorShowcase.Infrastructure/"]
COPY ["src/CleanArchitectureMediatorShowcase.WebAPI/CleanArchitectureMediatorShowcase.WebAPI.csproj", "src/CleanArchitectureMediatorShowcase.WebAPI/"]
COPY ["tests/CleanArchitectureMediatorShowcase.UnitTests/CleanArchitectureMediatorShowcase.UnitTests.csproj", "tests/CleanArchitectureMediatorShowcase.UnitTests/"]
COPY ["tests/CleanArchitectureMediatorShowcase.IntegrationTests/CleanArchitectureMediatorShowcase.IntegrationTests.csproj", "tests/CleanArchitectureMediatorShowcase.IntegrationTests/"]
COPY ["tests/CleanArchitectureMediatorShowcase.ArchitectureTests/CleanArchitectureMediatorShowcase.ArchitectureTests.csproj", "tests/CleanArchitectureMediatorShowcase.ArchitectureTests/"]

RUN dotnet restore CleanArchitectureMediatorShowcase.slnx

# Copy application source code
COPY . .

# Publish WebAPI
WORKDIR "/src/src/CleanArchitectureMediatorShowcase.WebAPI"
RUN dotnet publish "CleanArchitectureMediatorShowcase.WebAPI.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Runtime (Minimal secure Alpine image)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Prepare SQLite storage directory with proper permissions for non-root user
RUN mkdir -p /app/data && chown -R $APP_UID:$APP_UID /app
VOLUME ["/app/data"]

# Switch to non-root user
USER $APP_UID

# Copy compiled artifacts
COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .

# Environment configuration
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__Database="Data Source=/app/data/productcatalog.db"
EXPOSE 8080

# Health check probe
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "CleanArchitectureMediatorShowcase.WebAPI.dll"]
