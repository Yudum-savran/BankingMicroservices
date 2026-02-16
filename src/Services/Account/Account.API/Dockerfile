FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Account.API/Account.API.csproj", "Account.API/"]
COPY ["Account.Application/Account.Application.csproj", "Account.Application/"]
COPY ["Account.Domain/Account.Domain.csproj", "Account.Domain/"]
COPY ["Account.Infrastructure/Account.Infrastructure.csproj", "Account.Infrastructure/"]

RUN dotnet restore "Account.API/Account.API.csproj"

# Copy all source files
COPY . .

WORKDIR "/src/Account.API"
RUN dotnet build "Account.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Account.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Account.API.dll"]
