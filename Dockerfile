FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore (cached layer)
COPY src/Trivia.Shared/Trivia.Shared.csproj src/Trivia.Shared/
COPY src/Trivia.Api/Trivia.Api.csproj src/Trivia.Api/
RUN dotnet restore src/Trivia.Api/Trivia.Api.csproj

# Copy source and publish
COPY src/Trivia.Shared/ src/Trivia.Shared/
COPY src/Trivia.Api/ src/Trivia.Api/
RUN dotnet publish src/Trivia.Api/Trivia.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Trivia.Api.dll"]