# Stage 1: restore
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /app

COPY src/StackOverFlowLite.Domain/StackOverFlowLite.Domain.csproj             src/StackOverFlowLite.Domain/
COPY src/StackOverFlowLite.Application/StackOverFlowLite.Application.csproj   src/StackOverFlowLite.Application/
COPY src/StackOverFlowLite.Infrastructure/StackOverFlowLite.Infrastructure.csproj src/StackOverFlowLite.Infrastructure/
COPY src/StackOverFlowLite.API/StackOverFlowLite.API.csproj                   src/StackOverFlowLite.API/

RUN dotnet restore src/StackOverFlowLite.API/StackOverFlowLite.API.csproj

# Stage 2: build & publish
FROM restore AS publish
WORKDIR /app

COPY src/ src/

RUN dotnet publish src/StackOverFlowLite.API/StackOverFlowLite.API.csproj \
    --no-restore \
    -c Release \
    -o /app/publish

# Stage 3: runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "StackOverFlowLite.API.dll"]

