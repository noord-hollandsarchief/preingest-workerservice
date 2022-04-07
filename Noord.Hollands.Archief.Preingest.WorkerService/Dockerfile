#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
RUN apt-get update
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Noord.Hollands.Archief.Preingest.WorkerService/Noord.Hollands.Archief.Preingest.WorkerService.csproj", "Noord.Hollands.Archief.Preingest.WorkerService/"]
RUN dotnet restore "Noord.Hollands.Archief.Preingest.WorkerService/Noord.Hollands.Archief.Preingest.WorkerService.csproj"
COPY . .
WORKDIR "/src/Noord.Hollands.Archief.Preingest.WorkerService"
RUN dotnet build "Noord.Hollands.Archief.Preingest.WorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Noord.Hollands.Archief.Preingest.WorkerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Noord.Hollands.Archief.Preingest.WorkerService.dll"]