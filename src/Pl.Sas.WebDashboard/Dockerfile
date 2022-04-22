#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["src/Pl.Sas.WebDashboard/Pl.Sas.WebDashboard.csproj", "src/Pl.Sas.WebDashboard/"]
COPY ["src/Pl.Sas.Infrastructure/Pl.Sas.Infrastructure.csproj", "src/Pl.Sas.Infrastructure/"]
COPY ["src/Pl.Sas.Core/Pl.Sas.Core.csproj", "src/Pl.Sas.Core/"]
RUN dotnet restore "src/Pl.Sas.WebDashboard/Pl.Sas.WebDashboard.csproj"
COPY . .
WORKDIR "/src/src/Pl.Sas.WebDashboard"
RUN dotnet build "Pl.Sas.WebDashboard.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pl.Sas.WebDashboard.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pl.Sas.WebDashboard.dll"]
