FROM mcr.microsoft.com/dotnet/sdk:5.0.100-alpine3.12 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln .
COPY src/LiveDocs.Shared/*.csproj src/LiveDocs.Shared/
COPY src/LiveDocs.Server/*.csproj src/LiveDocs.Server/
COPY src/LiveDocs.Client/*.csproj src/LiveDocs.Client/
COPY src/LiveDocs.Generator/*.csproj src/LiveDocs.Generator/
COPY tests/LiveDocs.Shared.Tests/*.csproj tests/LiveDocs.Shared.Tests/
RUN dotnet restore LiveDocs.sln

# Copy everything else and build
COPY . ./
RUN dotnet publish src/LiveDocs.Server/*.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0.0-alpine3.12
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "LiveDocs.Server.dll"]
