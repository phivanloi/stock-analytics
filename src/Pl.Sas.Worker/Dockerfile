FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["src/Pl.Sas.Worker/Pl.Sas.Worker.csproj", "src/Pl.Sas.Worker/"]
COPY ["src/Pl.Sas.Infrastructure/Pl.Sas.Infrastructure.csproj", "src/Pl.Sas.Infrastructure/"]
COPY ["src/Pl.Sas.Core/Pl.Sas.Core.csproj", "src/Pl.Sas.Core/"]
RUN dotnet restore "src/Pl.Sas.Worker/Pl.Sas.Worker.csproj"
COPY . .
WORKDIR "/src/src/Pl.Sas.Worker"
RUN dotnet build "Pl.Sas.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pl.Sas.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pl.Sas.Worker.dll"]