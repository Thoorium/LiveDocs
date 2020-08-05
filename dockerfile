FROM mcr.microsoft.com/dotnet/sdk:5.0.100-preview.7-alpine AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln .
COPY src/LiveDocs.Shared/*.csproj src/LiveDocs.Shared/
COPY src/LiveDocs.WebApp/*.csproj src/LiveDocs.WebApp/
RUN dotnet restore LiveDocs.sln

# Copy everything else and build
COPY . ./
RUN dotnet publish src/LiveDocs.WebApp/*.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0.0-preview.7-alpine
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "LiveDocs.WebApp.dll"]
