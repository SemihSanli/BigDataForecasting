# 1. Aşama: Build (Derleme)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Sadece csproj dosyasını kopyalayıp restore ediyoruz (Cache optimizasyonu için)
COPY ["BigDataForecasting.API.csproj", "./"]
RUN dotnet restore "./BigDataForecasting.API.csproj"

# Tüm dosyaları kopyalayıp projeyi derliyoruz
COPY . .
RUN dotnet publish "BigDataForecasting.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Aşama: Run (Çalıştırma)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# .NET 8 varsayılan olarak 8080 portundan yayın yapar
EXPOSE 8080 
ENTRYPOINT ["dotnet", "BigDataForecasting.API.dll"]