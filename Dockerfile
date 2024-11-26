FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:d53ebf3481ea8ac8e4fa5c4213ae1f32a33e68e5b8181868edb11d0496a00432 AS base
WORKDIR /app
EXPOSE 8080

# Instalace curl pro health check
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:f91f9181d68b5ed2c8dea199dbbd2f6d172e60bdc43f8a67878b1ad140cf8d6b AS build
WORKDIR /src

# Kopírování NuGet konfigurace
COPY ["nuget.config", "./"]
COPY ["PatientDataApp.csproj", "./"]

# Obnovení závislostí s explicitním zdrojem
RUN dotnet restore "./PatientDataApp.csproj" --source https://api.nuget.org/v3/index.json

# Instalace Entity Framework CLI
RUN dotnet tool install --global dotnet-ef

# Kopírování zdrojového kódu a build
COPY . .
RUN dotnet build "PatientDataApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PatientDataApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Kopírování Entity Framework CLI z build image
COPY --from=build /root/.dotnet/tools /root/.dotnet/tools
ENV PATH="${PATH}:/root/.dotnet/tools"

# Vytvoření adresáře pro MRI snímky
RUN mkdir -p /app/data/mri-images && chmod 777 /app/data/mri-images

# Kopírování .env souboru
COPY .env .env
COPY .env.example .env.example

# Nastavení výchozích hodnot
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    PATH="$PATH:/root/.dotnet/tools" \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Použití pevných hodnot pro HEALTHCHECK
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "PatientDataApp.dll"]
