FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/InventoryApp.Api/InventoryApp.Api.csproj",             "src/InventoryApp.Api/"]
COPY ["src/InventoryApp.Application/InventoryApp.Application.csproj", "src/InventoryApp.Application/"]
COPY ["src/InventoryApp.Domain/InventoryApp.Domain.csproj",       "src/InventoryApp.Domain/"]
COPY ["src/InventoryApp.Infrastructure/InventoryApp.Infrastructure.csproj", "src/InventoryApp.Infrastructure/"]

RUN dotnet restore "src/InventoryApp.Api/InventoryApp.Api.csproj"

COPY . .

WORKDIR "/src/src/InventoryApp.Api"
RUN dotnet build "InventoryApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InventoryApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish \
    /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventoryApp.Api.dll"]