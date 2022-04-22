FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["logger/Pl.Sas.Logger/Pl.Sas.Logger.csproj", "logger/Pl.Sas.Logger/"]
RUN dotnet restore "logger/Pl.Sas.Logger/Pl.Sas.Logger.csproj"
COPY . .
WORKDIR "/src/logger/Pl.Sas.Logger"
RUN dotnet build "Pl.Sas.Logger.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pl.Sas.Logger.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pl.Sas.Logger.dll"]