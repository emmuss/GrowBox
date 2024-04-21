# Backend building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /
# Copy everything
COPY ./ ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish /GrowBox.Server/GrowBox.Server.csproj -c Release -o app -p:PublishTrimmed=false
# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
RUN chown -R 1000:1000 ./wwwroot
ENTRYPOINT ["dotnet", "./GrowBox.Server.dll"]