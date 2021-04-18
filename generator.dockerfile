FROM mcr.microsoft.com/dotnet/sdk:5.0.100-alpine3.12 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY LiveDocs.Generator.sln .
COPY src/LiveDocs.Shared/*.csproj src/LiveDocs.Shared/
COPY src/LiveDocs.Generator/*.csproj src/LiveDocs.Generator/
RUN dotnet restore LiveDocs.Generator.sln

# Copy everything else and build
COPY . ./
RUN dotnet publish src/LiveDocs.Generator/*.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0.0-alpine3.12
WORKDIR /LiveDocs
COPY --from=build-env /app/out .
# TODO: Temporary
COPY out/wwwroot .
ENTRYPOINT ["dotnet", "LiveDocs.Generator.dll"]
