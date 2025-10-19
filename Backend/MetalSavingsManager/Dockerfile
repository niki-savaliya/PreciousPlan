# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MetalSavingsManager.sln ./
COPY MetalSavingsManager/MetalSavingsManager.csproj MetalSavingsManager/
COPY MetalSavingsManager.Data/MetalSavingsManager.Data.csproj MetalSavingsManager.Data/
COPY MetalSavingsManager.Data.Model/MetalSavingsManager.Data.Model.csproj MetalSavingsManager.Data.Model/

# Restore once
RUN dotnet restore MetalSavingsManager.sln

# Copy source after restore
COPY . .

# Build (restore again to guarantee analyzers are available)
RUN dotnet build MetalSavingsManager.sln -c Release

# Publish
RUN dotnet publish MetalSavingsManager/MetalSavingsManager.csproj -c Release -o /app

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MetalSavingsManager.dll"]
