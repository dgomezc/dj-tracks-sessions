FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/DjTracksSessions.Web/DjTracksSessions.Web.csproj", "src/DjTracksSessions.Web/"]
COPY ["src/DjTracksSessions.Contracts/DjTracksSessions.Contracts.csproj", "src/DjTracksSessions.Contracts/"]
RUN dotnet restore "src/DjTracksSessions.Web/DjTracksSessions.Web.csproj"

COPY . .
RUN dotnet publish "src/DjTracksSessions.Web/DjTracksSessions.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DjTracksSessions.Web.dll"]
