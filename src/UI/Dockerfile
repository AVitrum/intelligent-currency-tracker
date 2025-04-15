FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 7135

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
COPY ["src/UI/UI.csproj", "UI/"]
COPY ["src/Shared/Shared.csproj", "Shared/"]
COPY ["src/Domain/Domain.csproj", "Domain/"]
RUN dotnet restore "UI/UI.csproj"
COPY . .
WORKDIR "/src/UI"
RUN dotnet build "UI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY src/UI/nginx.conf /etc/nginx/nginx.conf
