# Use the official ASP.NET Core runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5188

# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore any dependencies
COPY ["TMS_API.csproj", "./"]
RUN dotnet restore "./TMS_API.csproj"

# Copy the rest of the application files and build the app
COPY . .
WORKDIR "/src"
RUN dotnet build "TMS_API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "TMS_API.csproj" -c Release -o /app/publish

# Use the runtime image again to serve the app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TMS_API.dll"]
