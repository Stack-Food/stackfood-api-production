FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/StackFood.Production.API/StackFood.Production.API/StackFood.Production.API.csproj", "src/StackFood.Production.API/StackFood.Production.API/"]
COPY ["src/StackFood.Production.Application/StackFood.Production.Application/StackFood.Production.Application.csproj", "src/StackFood.Production.Application/StackFood.Production.Application/"]
COPY ["src/StackFood.Production.Domain/StackFood.Production.Domain/StackFood.Production.Domain.csproj", "src/StackFood.Production.Domain/StackFood.Production.Domain/"]
COPY ["src/StackFood.Production.Infrastructure/StackFood.Production.Infrastructure/StackFood.Production.Infrastructure.csproj", "src/StackFood.Production.Infrastructure/StackFood.Production.Infrastructure/"]

RUN dotnet restore "src/StackFood.Production.API/StackFood.Production.API/StackFood.Production.API.csproj"

# Copy all source files
COPY . .

# Build the application
WORKDIR "/src/src/StackFood.Production.API/StackFood.Production.API"
RUN dotnet build "StackFood.Production.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "StackFood.Production.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "StackFood.Production.API.dll"]
