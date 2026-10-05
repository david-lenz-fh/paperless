# Stage 1: Build
FROM node:24-alpine AS build
WORKDIR /app

# Dependency-Dateien kopieren
COPY package*.json ./
RUN npm ci

# Sourcecode kopieren und Angular bauen
COPY . .
RUN npm run build -- --configuration production

# Stage 2: Nginx Webserver
FROM nginx:alpine

COPY --from=build /app/dist/paperless-frontend/browser /usr/share/nginx/html

# Nginx-Konfiguration
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]