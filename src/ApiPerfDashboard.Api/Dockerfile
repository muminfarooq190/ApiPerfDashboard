FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./ApiPerfDashboard.Api.csproj ./
RUN dotnet restore ApiPerfDashboard.Api.csproj
COPY . .
RUN dotnet publish ApiPerfDashboard.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "ApiPerfDashboard.Api.dll"]
