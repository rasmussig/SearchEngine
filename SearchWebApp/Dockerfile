# Dockerfile for SearchWebApp (Blazor)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["SearchWebApp/SearchWebApp.csproj", "SearchWebApp/"]
RUN dotnet restore "SearchWebApp/SearchWebApp.csproj"

# Copy all source code
COPY Shared/ Shared/
COPY SearchWebApp/ SearchWebApp/

# Build
WORKDIR /src/SearchWebApp
RUN dotnet build "SearchWebApp.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "SearchWebApp.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

ENTRYPOINT ["dotnet", "SearchWebApp.dll"]
