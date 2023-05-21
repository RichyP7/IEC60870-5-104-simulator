#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 2404

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["IEC60870-5-104-simulator.API/IEC60870-5-104-simulator.API.csproj", "IEC60870-5-104-simulator.API/"]
RUN dotnet restore "IEC60870-5-104-simulator.API/IEC60870-5-104-simulator.API.csproj"
COPY . .
WORKDIR "/src/IEC60870-5-104-simulator.API"
RUN dotnet build "IEC60870-5-104-simulator.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IEC60870-5-104-simulator.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IEC60870-5-104-simulator.API.dll"]