FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8080
EXPOSE 443
EXPOSE 2404

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/IEC60870-5-104-simulator.API/*.csproj  IEC60870-5-104-simulator.API/
COPY src/IEC60870-5-104-simulator.Domain/*.csproj IEC60870-5-104-simulator.Domain/
COPY src/IEC60870-5-104-simulator.Infrastructure/*.csproj IEC60870-5-104-simulator.Infrastructure/
RUN dotnet restore IEC60870-5-104-simulator.API/IEC60870-5-104-simulator.API.csproj 
COPY /src /src
WORKDIR /src/IEC60870-5-104-simulator.API
RUN dotnet build -c Release -o /app/build

# test stage -- exposes optional entrypoint
# target entrypoint with: docker build --target test
FROM build AS test
WORKDIR /src
COPY src/Tests/IEC60870-5-104-simulator.Infrastructure.Tests/IEC60870-5-104-simulator.Infrastructure.Tests.csproj Tests/IEC60870-5-104-simulator.Infrastructure.Tests/
WORKDIR /src/Tests/IEC60870-5-104-simulator.Infrastructure.Tests/
RUN dotnet restore -v m 
COPY src/Tests/IEC60870-5-104-simulator.Infrastructure.Tests/ .

RUN dotnet build --no-restore
ENTRYPOINT ["dotnet", "test", "--logger:trx", "--no-build"]
# test end

# test service tests
FROM build AS test-service
WORKDIR /src
COPY src/Tests/IntegrationTests/IntegrationTests.csproj Tests/IntegrationTests/
WORKDIR /src/Tests/IntegrationTests/
RUN dotnet restore -v m 
COPY src/Tests/IntegrationTests/ .

RUN dotnet build --no-restore
ENTRYPOINT ["dotnet", "test", "--logger:trx", "--no-build"]
# test end


FROM build AS publish
RUN dotnet publish "IEC60870-5-104-simulator.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IEC60870-5-104-simulator.API.dll"]