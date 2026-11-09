# Build the ASP.NET Core application.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SunshineAttractions.Web.csproj", "./"]
RUN dotnet restore "SunshineAttractions.Web.csproj"

COPY . .
RUN dotnet publish "SunshineAttractions.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Keep the production image small and run only the published application.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render provides PORT at runtime (10000 by default). Its load balancer handles HTTPS.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000
ENTRYPOINT ["sh", "-c", "dotnet SunshineAttractions.Web.dll --urls http://0.0.0.0:${PORT:-10000}"]
