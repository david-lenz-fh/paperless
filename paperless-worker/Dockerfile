FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ["paperless-worker.csproj", "./"]
RUN dotnet restore "paperless-worker.csproj"

COPY . .
RUN dotnet publish "paperless-worker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

# OCR-Tools (Ghostscript & Tesseract) für Debian/Ubuntu-basiertes Runtime-Image installieren
RUN apt-get update && apt-get install -y --no-install-recommends \
    ghostscript \
    tesseract-ocr \
    tesseract-ocr-eng \
    tesseract-ocr-deu \
 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .


ENTRYPOINT ["dotnet", "paperless-worker.dll"]