# Dockerfile for LoadBalancer
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["LoadBalancer/LoadBalancer.csproj", "LoadBalancer/"]
RUN dotnet restore "LoadBalancer/LoadBalancer.csproj"

# Copy all source code
COPY Shared/ Shared/
COPY LoadBalancer/ LoadBalancer/

# Build
WORKDIR /src/LoadBalancer
RUN dotnet build "LoadBalancer.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "LoadBalancer.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

ENTRYPOINT ["dotnet", "LoadBalancer.dll"]
