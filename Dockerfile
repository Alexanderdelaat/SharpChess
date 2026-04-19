FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish src/SharpChess.Api/SharpChess.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app

ARG APP_VERSION=unknown
ENV ASPNETCORE_HTTP_PORTS=8080
ENV APP_VERSION=${APP_VERSION}

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SharpChess.Api.dll"]
