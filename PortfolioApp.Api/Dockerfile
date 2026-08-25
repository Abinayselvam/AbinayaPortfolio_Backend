FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the csproj file directly from your subfolder
COPY PortfolioApp.Api/*.csproj ./PortfolioApp.Api/
RUN dotnet restore PortfolioApp.Api/*.csproj

# Copy the rest of the files and build
COPY . ./
WORKDIR /src/PortfolioApp.Api
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "PortfolioApp.Api.dll"]