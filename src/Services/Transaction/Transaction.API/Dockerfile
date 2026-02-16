FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# csproj'leri KOPYALA
COPY ["Transaction.API/Transaction.API.csproj", "Transaction.API/"]
COPY ["Transaction.Application/Transaction.Application.csproj", "Transaction.Application/"]
COPY ["Transaction.Domain/Transaction.Domain.csproj", "Transaction.Domain/"]
COPY ["Transaction.Infrastructure/Transaction.Infrastructure.csproj", "Transaction.Infrastructure/"]

# restore
RUN dotnet restore "Transaction.API/Transaction.API.csproj"

# tüm kaynak kodu al
COPY . .

# API klasörüne geç
WORKDIR "/src/Transaction.API"

# build
RUN dotnet build "Transaction.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Transaction.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Transaction.API.dll"]