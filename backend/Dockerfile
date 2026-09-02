# --- Build stage: compiles the app using the full SDK image ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

# --- Runtime stage: much smaller image, just the ASP.NET runtime + published output ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render injects a PORT environment variable at container startup (not known at
# build time), so this has to be resolved in the shell at runtime rather than
# baked in as a Dockerfile ENV — hence the shell-form ENTRYPOINT below instead
# of a plain ASPNETCORE_URLS instruction.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:$PORT dotnet EmployeeLeaveManagement.dll"]
