# Multi-arch Dockerfile for Harvestry Edge
# Targets: linux/arm64 (HydroCore SOM), linux/amd64 (Generic IPC)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy Solution
COPY src/edge/Harvestry.Edge.sln ./src/edge/

# Copy all projects
COPY src/edge/src/Harvestry.Edge.Contracts/ ./src/edge/src/Harvestry.Edge.Contracts/
COPY src/edge/src/Harvestry.Edge.Engine/ ./src/edge/src/Harvestry.Edge.Engine/
COPY src/edge/src/Harvestry.Edge.HAL/ ./src/edge/src/Harvestry.Edge.HAL/
COPY src/edge/src/Harvestry.Edge.Storage/ ./src/edge/src/Harvestry.Edge.Storage/
COPY src/edge/src/Harvestry.Edge.OTA/ ./src/edge/src/Harvestry.Edge.OTA/
COPY src/edge/src/Harvestry.Edge.Adapters.Modbus/ ./src/edge/src/Harvestry.Edge.Adapters.Modbus/
COPY src/edge/src/Harvestry.Edge.Adapters.Mqtt/ ./src/edge/src/Harvestry.Edge.Adapters.Mqtt/
COPY src/edge/src/Harvestry.Edge.Adapters.EdgePod/ ./src/edge/src/Harvestry.Edge.Adapters.EdgePod/
COPY src/edge/src/Harvestry.Edge.API/ ./src/edge/src/Harvestry.Edge.API/
COPY src/edge/tests/ ./src/edge/tests/

# Restore
WORKDIR /src/src/edge
RUN dotnet restore Harvestry.Edge.sln

# Build API (which hosts the Engine)
WORKDIR /src/src/edge/src/Harvestry.Edge.API
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose HTTP (Local Dashboard)
EXPOSE 5000

# Install runtime dependencies for HW access (libgpiod, i2c-tools)
# RUN apt-get update && apt-get install -y libgpiod-dev i2c-tools

ENTRYPOINT ["dotnet", "Harvestry.Edge.API.dll"]
