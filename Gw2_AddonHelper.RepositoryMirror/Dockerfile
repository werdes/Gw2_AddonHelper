#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app
RUN mkdir -p /var/gw2-addonhelper-repositorymirror/output
RUN mkdir -p /var/gw2-addonhelper-repositorymirror/app
RUN mkdir -p /var/gw2-addonhelper-repositorymirror/log


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Gw2_AddonHelper.RepositoryMirror/Gw2_AddonHelper.RepositoryMirror.csproj", "Gw2_AddonHelper.RepositoryMirror/"]
RUN dotnet restore "Gw2_AddonHelper.RepositoryMirror/Gw2_AddonHelper.RepositoryMirror.csproj"
COPY . .
WORKDIR "/src/Gw2_AddonHelper.RepositoryMirror"
RUN dotnet build "Gw2_AddonHelper.RepositoryMirror.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gw2_AddonHelper.RepositoryMirror.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gw2_AddonHelper.RepositoryMirror.dll"]