# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["RestaurantPOS.api.csproj", "./"]
RUN dotnet restore "./RestaurantPOS.api.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "RestaurantPOS.api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "RestaurantPOS.api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RestaurantPOS.api.dll"]
