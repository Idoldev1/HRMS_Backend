# ── Build stage ────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj first for layer-cached restore
COPY HRMS.API.csproj .
RUN dotnet restore

# Copy everything else and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime stage ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create uploads directory
RUN mkdir -p /app/uploads /app/Logs

COPY --from=build /app/publish .

# Expose default port
EXPOSE 10000

# Render sets PORT env var; default to 10000 if not set
ENV ASPNETCORE_ENVIRONMENT=Production

# Use shell form so $PORT is expanded at runtime
CMD dotnet HRMS.API.dll --urls "http://+:${PORT:-10000}"
