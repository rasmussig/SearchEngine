# Dockerfile for SearchAPI
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["SearchAPI/SearchAPI.csproj", "SearchAPI/"]
RUN dotnet restore "SearchAPI/SearchAPI.csproj"

# Copy all source code
COPY Shared/ Shared/
COPY SearchAPI/ SearchAPI/

# Build
WORKDIR /src/SearchAPI
RUN dotnet build "SearchAPI.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "SearchAPI.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create Data directory for database
RUN mkdir -p /app/Data

# Expose port
EXPOSE 8080

ENTRYPOINT ["dotnet", "SearchAPI.dll"]
