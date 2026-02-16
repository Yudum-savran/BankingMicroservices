# 🚀 Kullanım Kılavuzu

## Projeyi Başlatma

### 1. Gereksinimler
```bash
# Docker Desktop yüklü olmalı
docker --version

# .NET 8.0 SDK yüklü olmalı
dotnet --version
```

### 2. Docker Container'ları Başlatma
```bash
# Tüm infrastructure servisleri başlat
cd banking-microservices
docker-compose up -d

# Servislerin durumunu kontrol et
docker-compose ps

# Logları izle
docker-compose logs -f
```

### 3. Erişim Bilgileri

**RabbitMQ Management UI:**
- URL: http://localhost:15672
- Username: admin
- Password: Admin123!

**Kibana (Elasticsearch UI):**
- URL: http://localhost:5601

**PostgreSQL Databases:**
- Account DB: localhost:5432
- Transaction DB: localhost:5433
- Customer DB: localhost:5434
- Auth DB: localhost:5435
- Username: admin
- Password: Admin123!

**Redis:**
- Host: localhost:6379
- Password: Redis123!

**Elasticsearch:**
- URL: http://localhost:9200

## API Kullanımı

### 1. Kullanıcı Kaydı (Register)

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecurePassword123!",
    "customerId": "550e8400-e29b-41d4-a716-446655440000"
  }'
```

**Response:**
```json
{
  "success": true,
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "message": "Registration successful"
}
```

### 2. Giriş Yapma (Login)

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecurePassword123!"
  }'
```

**Response:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "abc123def456...",
  "expiresAt": "2024-02-03T15:30:00Z",
  "message": "Login successful"
}
```

### 3. Hesap Oluşturma (Create Account)

```bash
# JWT token ile authenticated request
curl -X POST http://localhost:5000/api/accounts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "accountType": 1,
    "currency": "TRY",
    "dailyWithdrawLimit": 10000
  }'
```

**Response:**
```json
{
  "accountId": "789e0123-e89b-12d3-a456-426614174000",
  "accountNumber": "TR330000100000016345785634",
  "success": true,
  "message": "Account created successfully"
}
```

### 4. Hesap Bilgilerini Görüntüleme (Get Account)

```bash
# ID ile hesap sorgulama
curl -X GET http://localhost:5000/api/accounts/789e0123-e89b-12d3-a456-426614174000 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Hesap numarası ile sorgulama
curl -X GET http://localhost:5000/api/accounts/number/TR330000100000016345785634 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Müşterinin tüm hesaplarını görüntüleme
curl -X GET http://localhost:5000/api/accounts/customer/550e8400-e29b-41d4-a716-446655440000 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Response:**
```json
{
  "id": "789e0123-e89b-12d3-a456-426614174000",
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "accountNumber": "TR330000100000016345785634",
  "accountType": "Checking",
  "balance": 0.00,
  "currency": "TRY",
  "status": "Active",
  "createdAt": "2024-02-03T10:00:00Z",
  "dailyWithdrawLimit": 10000.00,
  "dailyWithdrawnAmount": 0.00
}
```

### 5. Para Yatırma (Deposit)

```bash
curl -X POST http://localhost:5000/api/accounts/789e0123-e89b-12d3-a456-426614174000/deposit \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "amount": 5000.00,
    "description": "Initial deposit"
  }'
```

**Response:**
```json
{
  "success": true,
  "newBalance": 5000.00,
  "message": "Deposit successful"
}
```

### 6. Para Çekme (Withdraw)

```bash
curl -X POST http://localhost:5000/api/accounts/789e0123-e89b-12d3-a456-426614174000/withdraw \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "amount": 1000.00,
    "description": "ATM withdrawal"
  }'
```

**Response:**
```json
{
  "success": true,
  "newBalance": 4000.00,
  "message": "Withdrawal successful"
}
```

### 7. Para Transferi (Transfer)

```bash
curl -X POST http://localhost:5000/api/accounts/789e0123-e89b-12d3-a456-426614174000/transfer \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "targetAccountId": "123e4567-e89b-12d3-a456-426614174111",
    "amount": 500.00,
    "description": "Payment to friend"
  }'
```

**Response:**
```json
{
  "success": true,
  "newBalance": 3500.00,
  "message": "Transfer successful"
}
```

## Event Flow Örneği

Bir para transferi işlemi şu adımları takip eder:

1. **Command:** `TransferMoneyCommand` API'ye gönderilir
2. **Domain Logic:** `BankAccount` aggregate'i business rules'ı kontrol eder
3. **Event Publishing:** `MoneyTransferredEvent` RabbitMQ'ya publish edilir
4. **Event Consumers:**
   - **Transaction Service:** İşlemi kaydeder
   - **Notification Service:** Email/SMS gönderir
5. **Logging:** Elasticsearch'e log kaydedilir
6. **Cache:** Redis cache güncellenir

## Monitoring ve Debugging

### RabbitMQ İzleme
```bash
# Management UI'dan kontrol et
http://localhost:15672

# Queues sekmesinden mesaj sayılarını gör
# Exchanges sekmesinden event routing'i kontrol et
```

### Elasticsearch Logları
```bash
# Kibana'dan logları görüntüle
http://localhost:5601

# Discover sekmesinden index pattern oluştur: account-service-logs-*
```

### Redis Cache Kontrolü
```bash
# Redis CLI'a bağlan
docker exec -it redis-cache redis-cli -a Redis123!

# Tüm keyleri listele
KEYS *

# Belirli bir key'in değerini gör
GET account:789e0123-e89b-12d3-a456-426614174000
```

### Database Kontrolü
```bash
# PostgreSQL'e bağlan
docker exec -it postgres-account psql -U admin -d AccountDB

# Hesapları listele
SELECT * FROM "Accounts";

# Çıkış
\q
```

## Postman Collection

Postman için hazır collection dosyası:

1. Postman'i aç
2. Import > File > `banking-api-collection.json` seç
3. Environment variables ayarla:
   - `base_url`: http://localhost:5000
   - `jwt_token`: Login response'undan aldığın token

## Test Senaryosu

### Tam Bir Banking Workflow:

```bash
# 1. Kullanıcı kaydı
register_response=$(curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "customerId": "550e8400-e29b-41d4-a716-446655440000"
  }')

# 2. Login ve JWT token al
login_response=$(curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }')

# Token'ı extract et
token=$(echo $login_response | jq -r '.token')

# 3. Hesap oluştur
account_response=$(curl -X POST http://localhost:5000/api/accounts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $token" \
  -d '{
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "accountType": 1,
    "currency": "TRY",
    "dailyWithdrawLimit": 10000
  }')

# Account ID'yi extract et
account_id=$(echo $account_response | jq -r '.accountId')

# 4. Para yatır
curl -X POST http://localhost:5000/api/accounts/$account_id/deposit \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $token" \
  -d '{
    "amount": 10000.00,
    "description": "Initial deposit"
  }'

# 5. Hesap bakiyesini kontrol et
curl -X GET http://localhost:5000/api/accounts/$account_id \
  -H "Authorization: Bearer $token"
```

## Troubleshooting

### Servis Bağlantı Sorunları
```bash
# Container'ların çalıştığını kontrol et
docker-compose ps

# Spesifik servis loglarını görüntüle
docker-compose logs account-service
docker-compose logs rabbitmq
docker-compose logs redis
```

### Database Migration Sorunları
```bash
# Manuel migration çalıştır
cd src/Services/Account/Account.API
dotnet ef database update
```

### RabbitMQ Bağlantı Hatası
```bash
# RabbitMQ'nun hazır olmasını bekle
docker-compose restart rabbitmq
docker-compose logs -f rabbitmq
```

## Clean Up

```bash
# Tüm container'ları durdur ve sil
docker-compose down

# Volume'ları da sil (dikkat: tüm data silinir)
docker-compose down -v

# Docker images'ları temizle
docker system prune -a
```

## Best Practices

1. **Token Yönetimi:** JWT token'ları güvenli bir yerde sakla (local storage yerine httpOnly cookies kullan)
2. **Error Handling:** Her zaman API response'larını kontrol et ve error'ları handle et
3. **Rate Limiting:** API Gateway'de rate limiting tanımlı, aşırı istek gönderme
4. **Logging:** Tüm önemli işlemleri Elasticsearch'te logla ve monitoring yap
5. **Security:** Production'da mutlaka HTTPS kullan ve secrets'ları environment variables olarak tut

## Production Deployment Checklist

- [ ] HTTPS sertifikası yapılandır
- [ ] Database connection strings'i environment variables olarak ayarla
- [ ] JWT secret'ı güçlü ve unique yap
- [ ] Redis ve RabbitMQ için strong passwords kullan
- [ ] Docker secrets kullan
- [ ] Health check endpoint'lerini monitoring sisteme bağla
- [ ] Backup stratejisi oluştur
- [ ] CI/CD pipeline kur
- [ ] Load balancer ekle
- [ ] Auto-scaling yapılandır
