# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the repository so project references and centrally managed build settings
# are available to restore and publish the API host.
COPY . .

RUN dotnet restore "src/SmartEstimate.Api/SmartEstimate.Api.csproj"
RUN dotnet publish "src/SmartEstimate.Api/SmartEstimate.Api.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# curl is used exclusively by the Compose readiness health check.
RUN apt-get update \
    && apt-get install --no-install-recommends --yes curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build --chown=app:app /app/publish .

USER app
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartEstimate.Api.dll"]
