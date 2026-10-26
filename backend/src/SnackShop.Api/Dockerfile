# Build context: repository root, so the shared .editorconfig (analyzer rules) applies exactly as in CI:
#   docker build -f backend/src/SnackShop.Api/Dockerfile -t snackshop-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY .editorconfig ./
COPY backend/global.json backend/Directory.Build.props backend/Directory.Packages.props backend/
COPY backend/src/SnackShop.Domain/SnackShop.Domain.csproj backend/src/SnackShop.Domain/
COPY backend/src/SnackShop.Application/SnackShop.Application.csproj backend/src/SnackShop.Application/
COPY backend/src/SnackShop.Infrastructure/SnackShop.Infrastructure.csproj backend/src/SnackShop.Infrastructure/
COPY backend/src/SnackShop.Api/SnackShop.Api.csproj backend/src/SnackShop.Api/
RUN dotnet restore backend/src/SnackShop.Api/SnackShop.Api.csproj

COPY backend/src/ backend/src/
RUN dotnet publish backend/src/SnackShop.Api/SnackShop.Api.csproj -c Release -o /app --no-restore

# Idempotent SQL for every migration, shipped inside the image so a deploy applies exactly the
# migrations of the code it runs (infra/scripts/deploy.sh). Uses DesignTimeDbContextFactory: no DB needed.
FROM build AS migrations
RUN dotnet tool install --global dotnet-ef --version "10.*"
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations script --idempotent --no-build --configuration Release \
      --project backend/src/SnackShop.Infrastructure --startup-project backend/src/SnackShop.Api \
      -o /migrations.sql

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
# The .NET base image already ships a non-root "app" user ($APP_UID).
RUN apk add --no-cache icu-libs tzdata wget \
    && mkdir -p /app/uploads /app/keys \
    && chown -R app /app
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=Asia/Ho_Chi_Minh \
    Storage__LocalPath=/app/uploads \
    DataProtection__KeysPath=/app/keys
USER app
COPY --from=build --chown=app /app .
COPY --from=migrations --chown=app /migrations.sql /app/migrations.sql
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=3s --start-period=20s CMD wget -qO- http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "SnackShop.Api.dll"]
