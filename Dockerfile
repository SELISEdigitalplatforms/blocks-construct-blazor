# syntax=docker/dockerfile:1

ARG SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:10.0
ARG RUNTIME_IMAGE=mcr.microsoft.com/dotnet/aspnet:10.0-alpine

FROM ${SDK_IMAGE} AS build
WORKDIR /src

# Install Node.js + npm for Tailwind build step invoked by Server.csproj (npm run css:build)
RUN apt-get update \
    && apt-get install -y --no-install-recommends nodejs npm \
    && rm -rf /var/lib/apt/lists/*

# Restore npm dependencies first for better layer caching
COPY package.json package-lock.json ./
RUN npm ci
RUN npm install --no-save @tailwindcss/oxide-linux-x64-gnu@4.2.2

# Copy project files and restore/publish
COPY . .
RUN dotnet restore ./src/Server/Server.csproj
RUN dotnet publish ./src/Server/Server.csproj -c Release -o /app/publish --no-restore

FROM ${RUNTIME_IMAGE} AS final
WORKDIR /app

COPY --from=build /app/publish .

# App binds to container port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Runtime configuration (pass these with -e or in compose/k8s)
# MICROSERVICE_API_BASE_URL
# X_BLOCKS_KEY
# PROJECT_SLUG

# Process-alive check: the Server has no health endpoint (PCR-0038), so this proves only that dotnet runs.
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
  CMD pgrep -x dotnet >/dev/null || exit 1

ENTRYPOINT ["dotnet", "Server.dll"]
