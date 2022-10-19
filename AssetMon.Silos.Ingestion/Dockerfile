#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 11111
EXPOSE 30000

#EXPOSE 80
#EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AssetMon.Silos.Ingestion/AssetMon.Silos.Ingestion.csproj", "AssetMon.Silos.Ingestion/"]
RUN dotnet restore "AssetMon.Silos.Ingestion/AssetMon.Silos.Ingestion.csproj"
COPY . .
WORKDIR "/src/AssetMon.Silos.Ingestion"
RUN dotnet build "AssetMon.Silos.Ingestion.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AssetMon.Silos.Ingestion.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AssetMon.Silos.Ingestion.dll"]