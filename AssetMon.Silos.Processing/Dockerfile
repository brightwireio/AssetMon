#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 11113
EXPOSE 30002

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AssetMon.Silos.Processing/AssetMon.Silos.Processing.csproj", "AssetMon.Silos.Processing/"]
RUN dotnet restore "AssetMon.Silos.Processing/AssetMon.Silos.Processing.csproj"
COPY . .
WORKDIR "/src/AssetMon.Silos.Processing"
RUN dotnet build "AssetMon.Silos.Processing.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AssetMon.Silos.Processing.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AssetMon.Silos.Processing.dll"]