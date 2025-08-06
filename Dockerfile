FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution file and project files
COPY *.sln .
COPY TelegramSativaBot.Domain/*.csproj ./TelegramSativaBot.Domain/
COPY TelegramSativaBot.Application/*.csproj ./TelegramSativaBot.Application/
COPY TelegramSativaBot.Infrastructure/*.csproj ./TelegramSativaBot.Infrastructure/
COPY TelegramSativaBot.Presentation/*.csproj ./TelegramSativaBot.Presentation/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build and publish
RUN dotnet publish TelegramSativaBot.Presentation/TelegramSativaBot.Presentation.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published files
COPY --from=build /out .

# Copy appsettings.json from build context
COPY TelegramSativaBot.Presentation/appsettings.json ./appsettings.json

# Expose port 8080
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "TelegramSativaBot.Presentation.dll"]
