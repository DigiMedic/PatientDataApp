FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopírování a obnovení závislostí
COPY ["PatientDataApp.csproj", "./"]
RUN dotnet restore

# Kopírování zdrojového kódu
COPY . .

# Publikování aplikace
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Nastavení proměnných prostředí
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80
ENTRYPOINT ["dotnet", "PatientDataApp.dll"] 