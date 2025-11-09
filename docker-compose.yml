# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln .
COPY ["AuthECAPI/AuthECAPI.csproj", "AuthECAPI/"]
RUN dotnet restore "AuthECAPI/AuthECAPI.csproj"

# Copy the rest of the files and build
COPY . .
WORKDIR "/app/AuthECAPI"
RUN dotnet publish "AuthECAPI.csproj" -c Release -o /out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .

# Expose port (default 8080 for Render/Container hosting)
EXPOSE 8080

# Set environment variable for Kestrel
ENV ASPNETCORE_URLS=http://+:8080

# Set the entry point
ENTRYPOINT ["dotnet", "AuthECAPI.dll"]
