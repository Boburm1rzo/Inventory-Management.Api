FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["InventoryApp.Api/InventoryApp.Api.csproj", "InventoryApp.Api/"]
COPY ["InventoryApp.Application/InventoryApp.Application.csproj", "InventoryApp.Application/"]
COPY ["InventoryApp.Domain/InventoryApp.Domain.csproj", "InventoryApp.Domain/"]
COPY ["InventoryApp.Infrastructure/InventoryApp.Infrastructure.csproj", "InventoryApp.Infrastructure/"]

RUN dotnet restore "InventoryApp.Api/InventoryApp.Api.csproj"

COPY . .

WORKDIR "/src/InventoryApp.Api"
RUN dotnet build "InventoryApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InventoryApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish \
    /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventoryApp.Api.dll"]