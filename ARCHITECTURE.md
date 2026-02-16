# 🏗️ Mimari Dokümantasyon

## Sistem Mimarisi

Bu proje, modern yazılım geliştirme prensiplerini uygulayan enterprise-grade bir banka mikroservis sistemidir.

## 📐 Mimari Prensipler

### 1. Domain-Driven Design (DDD)

Proje, DDD prensiplerine göre katmanlara ayrılmıştır:

```
┌─────────────────────────────────────────────────────────────┐
│                        API LAYER                            │
│  - Controllers (HTTP endpoints)                             │
│  - Middleware (JWT validation, logging)                     │
│  - DTOs (Data Transfer Objects)                             │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    APPLICATION LAYER                        │
│  - Commands (CQRS Write)                                    │
│  - Queries (CQRS Read)                                      │
│  - Command/Query Handlers                                   │
│  - Validators                                               │
│  - Application Services Interfaces                          │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                      DOMAIN LAYER                           │
│  - Entities (BankAccount)                                   │
│  - Value Objects (Money)                                    │
│  - Aggregates                                               │
│  - Domain Events                                            │
│  - Repository Interfaces                                    │
│  - Domain Services                                          │
│  - Business Rules                                           │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   INFRASTRUCTURE LAYER                      │
│  - EF Core DbContext                                        │
│  - Repository Implementations                               │
│  - RabbitMQ Event Bus                                       │
│  - Redis Cache Service                                      │
│  - Elasticsearch Service                                    │
│  - External API Integrations                                │
└─────────────────────────────────────────────────────────────┘
```

#### Domain Layer Detayları

**Aggregate Root: BankAccount**
```csharp
// Encapsulates business logic and invariants
public class BankAccount : AggregateRoot
{
    // Business rules enforced in domain
    public void Withdraw(decimal amount, string description)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Amount must be positive");
        
        if (Status != AccountStatus.Active)
            throw new InvalidOperationException("Account not active");
        
        if (Balance.Amount < amount)
            throw new InvalidOperationException("Insufficient balance");
        
        // Daily limit check
        ResetDailyLimitIfNeeded();
        if (DailyWithdrawnAmount + amount > DailyWithdrawLimit)
            throw new InvalidOperationException("Daily limit exceeded");
        
        // Execute withdrawal
        Balance = Balance.Subtract(amount);
        DailyWithdrawnAmount += amount;
        
        // Raise domain event
        AddDomainEvent(new MoneyWithdrawnEvent(...));
    }
}
```

**Value Object: Money**
```csharp
// Immutable, no identity
public class Money : IEquatable<Money>
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    
    // Value objects are compared by their values
    public bool Equals(Money? other) =>
        other != null && 
        Amount == other.Amount && 
        Currency == other.Currency;
}
```

### 2. CQRS (Command Query Responsibility Segregation)

Okuma ve yazma işlemleri ayrılmıştır:

```
WRITE SIDE (Commands)              READ SIDE (Queries)
─────────────────────              ───────────────────
┌──────────────────┐               ┌─────────────────┐
│  CreateAccount   │               │  GetAccountById │
│  DepositMoney    │               │  GetAccounts    │
│  WithdrawMoney   │               │  SearchAccount  │
│  TransferMoney   │               └─────────────────┘
└──────────────────┘                        ↓
         ↓                          ┌─────────────────┐
┌──────────────────┐               │   Redis Cache   │
│  PostgreSQL      │               │  (Fast Reads)   │
│  (Source Truth)  │               └─────────────────┘
└──────────────────┘
         ↓
┌──────────────────┐
│  Domain Events   │
│  (RabbitMQ)      │
└──────────────────┘
```

**Command Example:**
```csharp
// Write operation
public class DepositMoneyCommand : IRequest<DepositMoneyResponse>
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
}

// Handler
public class DepositMoneyCommandHandler : IRequestHandler<...>
{
    public async Task<DepositMoneyResponse> Handle(...)
    {
        // 1. Get aggregate
        var account = await _repository.GetByIdAsync(...);
        
        // 2. Execute domain logic
        account.Deposit(request.Amount, request.Description);
        
        // 3. Save changes
        await _unitOfWork.SaveChangesAsync();
        
        // 4. Publish events
        foreach (var @event in account.DomainEvents)
            await _eventBus.PublishAsync(@event);
        
        return new DepositMoneyResponse { ... };
    }
}
```

**Query Example:**
```csharp
// Read operation with caching
public class GetAccountByIdQuery : IRequest<AccountDto?>
{
    public Guid AccountId { get; set; }
}

// Handler
public class GetAccountByIdQueryHandler : IRequestHandler<...>
{
    public async Task<AccountDto?> Handle(...)
    {
        // 1. Try cache first
        var cached = await _cache.GetAsync<AccountDto>(key);
        if (cached != null) return cached;
        
        // 2. Get from database
        var account = await _repository.GetByIdAsync(...);
        
        // 3. Map to DTO and cache
        var dto = MapToDto(account);
        await _cache.SetAsync(key, dto, TimeSpan.FromMinutes(5));
        
        return dto;
    }
}
```

### 3. Event-Driven Architecture

Mikroservisler arası iletişim asenkron event'ler üzerinden gerçekleşir:

```
┌─────────────────────────────────────────────────────────────┐
│                    EVENT FLOW EXAMPLE                       │
└─────────────────────────────────────────────────────────────┘

USER ACTION: Transfer Money
         ↓
┌─────────────────┐
│ API Gateway     │ ← JWT Authentication
└─────────────────┘
         ↓
┌─────────────────┐
│ Account Service │
│ TransferMoney   │ ← Business Logic (DDD)
└─────────────────┘
         ↓
┌─────────────────────────────────────────────┐
│         RabbitMQ Event Bus                  │
│  Exchange: banking_events (Topic)           │
│  Routing Key: account.moneytransferred      │
└─────────────────────────────────────────────┘
         ↓  (Fanout to multiple consumers)
    ┌────┴────┬────────────┬─────────────┐
    ↓         ↓            ↓             ↓
┌────────┐ ┌──────┐ ┌──────────┐ ┌──────────┐
│Transaction│Notification│Fraud    │Elasticsearch│
│Service  │ │Service  │ │Detection│ │Logger   │
└────────┘ └──────┘ └──────────┘ └──────────┘
    │         │          │            │
    ↓         ↓          ↓            ↓
  Record   Send    Analyze      Index
 History   Email   Pattern      Logs
```

**Event Publishing:**
```csharp
// Domain Event
public class MoneyTransferredEvent : DomainEvent
{
    public Guid SourceAccountId { get; }
    public Guid TargetAccountId { get; }
    public decimal Amount { get; }
}

// RabbitMQ Publisher
public class RabbitMQEventBus : IEventBus
{
    public async Task PublishAsync<T>(T @event, ...) where T : IDomainEvent
    {
        var routingKey = $"account.{@event.GetType().Name.ToLower()}";
        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);
        
        _channel.BasicPublish(
            exchange: "banking_events",
            routingKey: routingKey,
            body: body);
    }
}
```

### 4. Database Per Service Pattern

Her mikroservis kendi veritabanına sahiptir:

```
┌──────────────────┐     ┌──────────────────┐
│ Account Service  │────▶│ PostgreSQL       │
│                  │     │ AccountDB        │
└──────────────────┘     └──────────────────┘

┌──────────────────┐     ┌──────────────────┐
│Transaction Service│────▶│ PostgreSQL       │
│                  │     │ TransactionDB    │
└──────────────────┘     └──────────────────┘

┌──────────────────┐     ┌──────────────────┐
│ Customer Service │────▶│ PostgreSQL       │
│                  │     │ CustomerDB       │
└──────────────────┘     └──────────────────┘

┌──────────────────┐     ┌──────────────────┐
│ Auth Service     │────▶│ PostgreSQL       │
│                  │     │ AuthDB           │
└──────────────────┘     └──────────────────┘
```

**Avantajları:**
- Loose coupling
- Independent scaling
- Technology diversity
- Fault isolation

### 5. Caching Strategy (Redis)

Redis, read performance için kullanılır:

```
┌──────────────┐
│ Query Request│
└──────────────┘
       ↓
  ┌────────┐
  │ Cache? │
  └────────┘
    ↓     ↓
  YES    NO
    ↓     ↓
  ┌────┐ ┌────────┐
  │Return│Database│
  └────┘ └────────┘
           ↓
       ┌────────┐
       │ Cache  │
       │ Result │
       └────────┘
```

**Cache Patterns:**

1. **Cache-Aside:**
```csharp
// Try cache first
var data = await _cache.GetAsync<T>(key);
if (data == null) {
    data = await _database.GetAsync(key);
    await _cache.SetAsync(key, data, expiry);
}
return data;
```

2. **Write-Through:**
```csharp
// Update both database and cache
await _database.UpdateAsync(data);
await _cache.SetAsync(key, data, expiry);
```

3. **Cache Invalidation:**
```csharp
// Clear cache when data changes
await _database.UpdateAsync(data);
await _cache.RemoveAsync($"account:{accountId}");
await _cache.RemoveAsync($"customer:{customerId}:accounts");
```

### 6. API Gateway Pattern (Ocelot)

Tüm client istekleri API Gateway üzerinden geçer:

```
┌────────────┐
│   Client   │
└────────────┘
      ↓
┌────────────────────────────────┐
│       API Gateway              │
│  - JWT Validation              │
│  - Rate Limiting               │
│  - Request Routing             │
│  - Load Balancing              │
└────────────────────────────────┘
      ↓
  ┌───┴───┬─────────┬────────┐
  ↓       ↓         ↓        ↓
┌────┐ ┌────┐ ┌────┐ ┌────┐
│Auth││Acct││Txn ││Cust││
└────┘ └────┘ └────┘ └────┘
```

**Ocelot Configuration:**
```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/accounts/{everything}",
      "UpstreamPathTemplate": "/api/accounts/{everything}",
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      },
      "RateLimitOptions": {
        "EnableRateLimiting": true,
        "Period": "1m",
        "Limit": 100
      }
    }
  ]
}
```

### 7. Authentication & Authorization (JWT)

```
┌─────────────────────────────────────────────────────────┐
│                  Authentication Flow                    │
└─────────────────────────────────────────────────────────┘

1. User sends credentials
   ↓
2. Auth Service validates
   ↓
3. Generate JWT Token
   ↓
4. Store refresh token in Redis
   ↓
5. Return tokens to client
   ↓
6. Client includes JWT in requests
   ↓
7. API Gateway validates JWT
   ↓
8. Forward to microservice
```

**JWT Token Structure:**
```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user-id",
    "email": "user@example.com",
    "role": "Customer",
    "customerId": "customer-id",
    "exp": 1234567890
  },
  "signature": "..."
}
```

### 8. Logging & Monitoring (Elasticsearch + Kibana)

```
┌────────────────────────────────────────┐
│        Logging Architecture            │
└────────────────────────────────────────┘

Microservices
     ↓ (Serilog)
┌────────────┐
│Elasticsearch│ ← Structured logs
└────────────┘
     ↓
┌────────────┐
│  Kibana    │ ← Visualization & Search
└────────────┘
```

**Structured Logging:**
```csharp
Log.Information(
    "Account created. AccountId: {AccountId}, CustomerId: {CustomerId}",
    account.Id,
    account.CustomerId);

// Creates searchable fields in Elasticsearch
{
  "@timestamp": "2024-02-03T10:00:00Z",
  "level": "Information",
  "message": "Account created...",
  "accountId": "123...",
  "customerId": "456...",
  "serviceName": "AccountService"
}
```

## 🔒 Security Architecture

### Defense in Depth:

1. **Network Level:** Docker network isolation
2. **API Gateway:** Rate limiting, CORS
3. **Authentication:** JWT tokens
4. **Authorization:** Role-based access
5. **Database:** Parameterized queries (SQL injection prevention)
6. **Secrets:** Environment variables, Docker secrets

## 📊 Performance Optimizations

### 1. Redis Caching
- TTL: 5 minutes for account data
- Invalidation on writes
- Pattern-based cleanup

### 2. Database Indexing
- Account number (unique)
- Customer ID
- Status
- Created date

### 3. Connection Pooling
- EF Core connection pooling
- Redis connection multiplexer
- RabbitMQ connection reuse

### 4. Asynchronous Processing
- All I/O operations async
- Event-driven notifications
- Background job processing

## 🔄 Transaction Management

### Distributed Transactions:

```csharp
// Money transfer between accounts
await _unitOfWork.BeginTransactionAsync();

try {
    // Debit source account
    sourceAccount.Withdraw(amount, description);
    await _repository.UpdateAsync(sourceAccount);
    
    // Credit target account
    targetAccount.Deposit(amount, description);
    await _repository.UpdateAsync(targetAccount);
    
    // Commit transaction
    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();
    
    // Publish events
    await _eventBus.PublishAsync(...);
}
catch {
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

## 📈 Scalability

### Horizontal Scaling:

```
        Load Balancer
             ↓
    ┌────────┴────────┐
    ↓                 ↓
Account Service  Account Service
  Instance 1       Instance 2
    ↓                 ↓
  ┌──────────────────┐
  │  PostgreSQL      │
  │  (Shared DB)     │
  └──────────────────┘
```

### Auto-scaling Ready:
- Stateless services
- Shared cache (Redis)
- Message broker (RabbitMQ)
- Centralized logging

## 🎯 Design Patterns Kullanılan

1. **Repository Pattern:** Data access abstraction
2. **Unit of Work Pattern:** Transaction management
3. **CQRS Pattern:** Command/Query separation
4. **Mediator Pattern:** MediatR for CQRS
5. **Factory Pattern:** Domain entity creation
6. **Event Sourcing:** Domain events
7. **Gateway Pattern:** API Gateway (Ocelot)
8. **Circuit Breaker:** Fault tolerance (future)
9. **Saga Pattern:** Distributed transactions (future)

## 🚀 Production Readiness

### Implemented:
✅ Health checks
✅ Structured logging
✅ Error handling
✅ Input validation
✅ Authentication/Authorization
✅ Rate limiting
✅ Caching
✅ Database migrations
✅ Docker containerization

### Future Enhancements:
- [ ] Circuit breaker (Polly)
- [ ] Distributed tracing (Jaeger)
- [ ] Service mesh (Istio)
- [ ] Kubernetes deployment
- [ ] Blue-green deployment
- [ ] Canary releases
- [ ] Automated testing
- [ ] Performance testing
