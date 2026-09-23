# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Projektdateien kopieren
COPY api/*.csproj ./api/
COPY business_layer/*.csproj ./business_layer/
COPY data/*.csproj ./data/

# Wiederherstellung der NuGet-Pakete für API
RUN dotnet restore ./api/*.csproj

# Den gesamten Quellcode kopieren
COPY . .

# Kompilieren und nach /app/publish ausgeben
WORKDIR /src/api
RUN dotnet publish -c Release -o /app/publish

# 2. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Starte die generierte DLL
ENTRYPOINT ["dotnet", "api.dll"]