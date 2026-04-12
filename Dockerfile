# Base stage for runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all project files for restoration
COPY ["Bookify.API/Bookify.API.csproj", "Bookify.API/"]
COPY ["Bookify.Application/Bookify.Application.csproj", "Bookify.Application/"]
COPY ["Bookify.Domain/Bookify.Domain.csproj", "Bookify.Domain/"]
COPY ["Bookify.Infrastructure/Bookify.Infrastructure.csproj", "Bookify.Infrastructure/"]
COPY ["Bookify.ServiceDefaults/Bookify.ServiceDefaults.csproj", "Bookify.ServiceDefaults/"]

# Restore dependencies
RUN dotnet restore "./Bookify.API/Bookify.API.csproj"

# Copy the entire source
COPY . .
WORKDIR "/src/Bookify.API"

# Build the API
RUN dotnet build "./Bookify.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Bookify.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bookify.API.dll"]
