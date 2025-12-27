# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AccountingSystem.sln ./
COPY src/AccountingSystem.Api/AccountingSystem.Api.csproj src/AccountingSystem.Api/
RUN dotnet restore src/AccountingSystem.Api/AccountingSystem.Api.csproj

COPY src/AccountingSystem.Api/ src/AccountingSystem.Api/
RUN dotnet publish src/AccountingSystem.Api/AccountingSystem.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 8080
ENTRYPOINT ["dotnet", "AccountingSystem.Api.dll"]
