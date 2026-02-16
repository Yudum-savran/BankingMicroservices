FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Customer.API/Customer.API.csproj", "Customer.API/"]
COPY ["Customer.Application/Customer.Application.csproj", "Customer.Application/"]
COPY ["Customer.Domain/Customer.Domain.csproj", "Customer.Domain/"]
COPY ["Customer.Infrastructure/Customer.Infrastructure.csproj", "Customer.Infrastructure/"]

RUN dotnet restore "Customer.API/Customer.API.csproj"

COPY . .
WORKDIR "/src/Customer.API"
RUN dotnet build "Customer.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Customer.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Customer.API.dll"]