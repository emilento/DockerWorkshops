FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
# ENV ASPNETCORE_ENVIRONMENT=Development

RUN apt-get update
RUN apt-get --yes install curl

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DockerWorkshops.csproj", "./"]
RUN dotnet restore "DockerWorkshops.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "DockerWorkshops.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DockerWorkshops.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# https://docs.docker.com/engine/reference/builder/#healthcheck
HEALTHCHECK --interval=5s --timeout=10s --retries=3 CMD curl --silent --fail http://localhost:5000/healthz || exit 1

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DockerWorkshops.dll"]
