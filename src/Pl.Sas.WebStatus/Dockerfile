FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["src/Pl.Sas.WebStatus/Pl.Sas.WebStatus.csproj", "src/Pl.Sas.WebStatus/"]
RUN dotnet restore "src/Pl.Sas.WebStatus/Pl.Sas.WebStatus.csproj"
COPY . .
WORKDIR "/src/src/Pl.Sas.WebStatus"
RUN dotnet build "Pl.Sas.WebStatus.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pl.Sas.WebStatus.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pl.Sas.WebStatus.dll"]