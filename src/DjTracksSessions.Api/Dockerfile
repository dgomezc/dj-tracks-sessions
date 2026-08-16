FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/DjTracksSessions.Api/DjTracksSessions.Api.csproj", "src/DjTracksSessions.Api/"]
COPY ["src/DjTracksSessions.Contracts/DjTracksSessions.Contracts.csproj", "src/DjTracksSessions.Contracts/"]
COPY ["src/DjTrackSessions.Domain/DjTrackSessions.Domain.csproj", "src/DjTrackSessions.Domain/"]
COPY ["src/DjTrackSessions.Infrastructure/DjTrackSessions.Infrastructure.csproj", "src/DjTrackSessions.Infrastructure/"]
RUN dotnet restore "src/DjTracksSessions.Api/DjTracksSessions.Api.csproj"

COPY . .
RUN dotnet publish "src/DjTracksSessions.Api/DjTracksSessions.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DjTracksSessions.Api.dll"]
