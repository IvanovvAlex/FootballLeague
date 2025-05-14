FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY Directory.Packages.props . 
COPY FootballLeague.API/*.csproj FootballLeague.API/
COPY FootballLeague.Data/*.csproj FootballLeague.Data/
COPY FootballLeague.Domain/*.csproj FootballLeague.Domain/
COPY FootballLeague.Common/*.csproj FootballLeague.Common/
COPY FootballLeague.Tests/*.csproj FootballLeague.Tests/
COPY FootballLeague.Shared/*.csproj FootballLeague.Shared/
RUN dotnet restore FootballLeague.API/FootballLeague.API.csproj

COPY . .
RUN dotnet publish FootballLeague.API/FootballLeague.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5080
EXPOSE 5080

ENTRYPOINT ["dotnet", "FootballLeague.API.dll"]
