FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8003

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
COPY ["src/DevUI/DevUI.csproj", "DevUI/"]
COPY ["src/Shared/Shared.csproj", "Shared/"]
COPY ["src/Domain/Domain.csproj", "Domain/"]
RUN dotnet restore "DevUI/DevUI.csproj"
COPY . .
WORKDIR "/src/DevUI"
RUN dotnet build "DevUI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DevUI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY src/DevUI/nginx.conf /etc/nginx/nginx.conf
