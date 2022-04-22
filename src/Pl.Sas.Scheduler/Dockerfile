FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["src/Pl.Sas.Scheduler/Pl.Sas.Scheduler.csproj", "src/Pl.Sas.Scheduler/"]
COPY ["src/Pl.Sas.Infrastructure/Pl.Sas.Infrastructure.csproj", "src/Pl.Sas.Infrastructure/"]
COPY ["src/Pl.Sas.Core/Pl.Sas.Core.csproj", "src/Pl.Sas.Core/"]
RUN dotnet restore "src/Pl.Sas.Scheduler/Pl.Sas.Scheduler.csproj"
COPY . .
WORKDIR "/src/src/Pl.Sas.Scheduler"
RUN dotnet build "Pl.Sas.Scheduler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pl.Sas.Scheduler.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pl.Sas.Scheduler.dll"]