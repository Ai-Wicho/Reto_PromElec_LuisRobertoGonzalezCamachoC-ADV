# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["TiendaPromElec.csproj", "./"]
RUN dotnet restore "TiendaPromElec.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "TiendaPromElec.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TiendaPromElec.csproj" -c Release -o /app/publish

#  (.NET Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .


# 1. In-memory DB
ENV USE_IN_MEMORY_DB=true
# 2. Container port 80
ENV ASPNETCORE_HTTP_PORTS=80
# 3. Ver Swagger
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "TiendaPromElec.dll"]