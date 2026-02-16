# 🏦 Banking Mikroservis Sistemi

## 📋 Proje Genel Bakış

Modern banka uygulaması - Domain-Driven Design (DDD) prensiplerine göre tasarlanmış, enterprise-grade mikroservis mimarisi.

## 🏗️ Mimari Tasarım

### Mikroservisler

1. **Account Service** (Hesap Yönetimi)
   - Banka hesaplarının oluşturulması ve yönetimi
   - CQRS pattern ile okuma/yazma ayrımı
   - PostgreSQL (Write DB) + Redis (Read Cache)

2. **Transaction Service** (İşlem Yönetimi)
   - Para transferleri, yatırma, çekme işlemleri
   - Event Sourcing ile transaction history
   - PostgreSQL + Elasticsearch (Transaction Search)

3. **Customer Service** (Müşteri Yönetimi)
   - Müşteri bilgileri ve KYC
   - PostgreSQL
   - Redis cache

4. **Auth Service** (Kimlik Doğrulama)
   - JWT tabanlı authentication
   - Redis token store
   - PostgreSQL kullanıcı veritabanı

5. **Notification Service** (Bildirim Servisi)
   - Email/SMS bildirimleri
   - RabbitMQ consumer
   - Event-driven notifications

7. **API Gateway**
   - Ocelot API Gateway
   - JWT validation
   - Rate limiting ve routing

## 🛠️ Teknoloji Stack

- **.NET 8.0**: Mikroservisler
- **PostgreSQL**: Primary database
- **Redis**: Caching ve session management
- **RabbitMQ**: Message broker ve event bus
- **Elasticsearch**: Log yönetimi ve arama
- **Docker & Docker Compose**: Konteynerizasyon
- **JWT**: Authentication token
- **MediatR**: CQRS implementation
- **Entity Framework Core**: ORM
- **FluentValidation**: Validation
- **Serilog**: Structured logging
- **AutoMapper**: Object mapping

## 📁 Proje Yapısı

```
banking-microservices/
├── src/
│   ├── Services/
│   │   ├── Account/
│   │   │   ├── Account.API/
│   │   │   ├── Account.Application/
│   │   │   ├── Account.Domain/
│   │   │   └── Account.Infrastructure/
│   │   ├── Transaction/
│   │   ├── Customer/
│   │   ├── Auth/
│   │   ├── Notification/
│   │
│   ├── ApiGateway/
│   └── BuildingBlocks/
│       ├── EventBus/
│       ├── Common/
│       └── Infrastructure/
├── docker-compose.yml
└── README.md
```

## 🎯 DDD Katmanları

### Domain Layer
- Entities
- Value Objects
- Aggregates
- Domain Events
- Repository Interfaces
- Domain Services

### Application Layer
- Commands (CQRS Write)
- Queries (CQRS Read)
- Command/Query Handlers
- DTOs
- Validators
- Application Services

### Infrastructure Layer
- Repository Implementations
- EF Core DbContext
- External Service Integrations
- Message Bus Implementation

### API Layer
- Controllers
- Middleware
- Filters
- Configuration

## 🚀 Nasıl Çalıştırılır

### Gereksinimler
- Docker Desktop
- .NET 8.0 SDK
- Visual Studio 2022 / VS Code

### Çalıştırma

```bash
# Docker container'ları başlat
docker-compose up -d

# Veritabanı migration'ları çalıştır
dotnet ef database update --project src/Services/Account/Account.API

# Tüm servisleri başlat
dotnet run --project src/ApiGateway/ApiGateway.csproj
```

## 📊 Event Flow Örneği

```
1. Kullanıcı para transferi başlatır (API Gateway)
2. Transaction Service komutu işler
3. AccountDebitedEvent publish edilir (RabbitMQ)
4. Account Service hesap bakiyesini günceller
5. Notification Service bildirim gönderir
6. Elasticsearch'e log kaydedilir
```

## 🔒 Güvenlik

- JWT Bearer Authentication
- Role-based Authorization
- API Rate Limiting
- HTTPS Enforcement
- SQL Injection Protection
- XSS Protection

## 📈 Monitoring

- Elasticsearch + Kibana (Logging)
- Health Checks
- Distributed Tracing

## 🧪 Test Stratejisi

- Unit Tests (Domain & Application Layer)
- Integration Tests (Infrastructure)
- End-to-End Tests (API)
- Load Tests

## 📝 Lisans

MIT License
