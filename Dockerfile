# Use the official ASP.NET Core runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5188

# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the main project and test project files
COPY ["API/TMS_IDP.csproj", "API/"]
COPY ["Tests/Tests.csproj", "Tests/"]

# Restore dependencies for both the main API and test project
RUN dotnet restore "API/TMS_IDP.csproj"
RUN dotnet restore "Tests/Tests.csproj"

# Copy the rest of the application files
COPY . .

# Build the application
RUN dotnet build "API/TMS_IDP.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "API/TMS_IDP.csproj" -c Release -o /app/publish

# Use the runtime image again to serve the app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TMS_IDP.dll"]
