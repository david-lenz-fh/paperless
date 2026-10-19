FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ["paperless-worker.csproj", "./"]
RUN dotnet restore "paperless-worker.csproj"

COPY . .
RUN dotnet publish "paperless-worker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "paperless-worker.dll"]