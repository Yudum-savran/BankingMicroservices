# 🚀 Visual Studio ile Çalıştırma Rehberi

## 📋 Gereksinimler

### 1. Yazılımlar
- **Visual Studio 2022** (Community, Professional veya Enterprise)
- **Docker Desktop** (Windows/Mac)
- **.NET 8.0 SDK**
- **Git** (opsiyonel)

### 2. Visual Studio Workloads
Visual Studio Installer'dan şunları yükleyin:
- ✅ ASP.NET and web development
- ✅ .NET desktop development
- ✅ Container development tools

---

## 🎯 Adım Adım Kurulum

### ADIM 1: Docker Desktop'ı Başlatın

```powershell
# Docker'ın çalıştığını kontrol edin
docker --version
docker ps

# Çıktı: Docker version 24.0.x ...
```

**Önemli:** Docker Desktop'ın çalışıyor olması şart!

---

### ADIM 2: ZIP'i Açın ve Projeyi Açın

```
1. banking-microservices.zip dosyasını C:\Projects\ klasörüne çıkarın
2. Visual Studio 2022'yi açın
3. File > Open > Folder seçin
4. C:\Projects\banking-microservices klasörünü seçin
```

Ya da Solution açmak için:
```
File > Open > Project/Solution
banking-microservices klasörüne gidin
src\Services\Account\Account.API\Account.API.csproj seçin
```

---

### ADIM 3: Infrastructure Servislerini Başlatın

Visual Studio'da **Terminal** açın (View > Terminal) veya ayrı bir PowerShell/CMD açın:

```powershell
cd C:\Projects\banking-microservices

# Docker Compose ile tüm infrastructure'ı başlat
docker-compose up -d postgres-account postgres-transaction postgres-customer postgres-auth redis rabbitmq elasticsearch kibana

# Servislerin durumunu kontrol et
docker-compose ps
```

**Beklenen Çıktı:**
```
NAME                STATUS              PORTS
postgres-account    Up (healthy)        0.0.0.0:5432->5432/tcp
postgres-transaction Up (healthy)       0.0.0.0:5433->5432/tcp
postgres-customer   Up (healthy)        0.0.0.0:5434->5432/tcp
postgres-auth       Up (healthy)        0.0.0.0:5435->5432/tcp
redis-cache         Up (healthy)        0.0.0.0:6379->6379/tcp
rabbitmq            Up (healthy)        0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
elasticsearch       Up (healthy)        0.0.0.0:9200->9200/tcp
kibana              Up (healthy)        0.0.0.0:5601->5601/tcp
```

⏳ **İlk çalıştırmada 2-3 dakika bekleyin** (Docker images indirilecek)

---

### ADIM 4: NuGet Paketlerini Restore Edin

Visual Studio'da:

```
1. Solution Explorer'da projeye sağ tık
2. "Restore NuGet Packages" seçin
```

Ya da Terminal'de:
```powershell
# Account Service için
cd src\Services\Account\Account.API
dotnet restore

# Auth Service için
cd ..\..\..\..\Auth\Auth.API
dotnet restore

# API Gateway için
cd ..\..\..\..\ApiGateway
dotnet restore
```

---

### ADIM 5: Servisleri Çalıştırın

#### YÖNTEM 1: Visual Studio'da Tek Tek Çalıştırma

**A) Auth Service'i başlat:**
```
1. Solution Explorer'da Auth.API projesine sağ tık
2. "Set as Startup Project" seçin
3. F5 veya "Start Debugging" butonuna bas
4. Tarayıcıda http://localhost:5004 açılacak
```

**B) Account Service'i başlat:**
```
1. Yeni bir Visual Studio instance açın (ya da aynıda Multiple Startup Projects ayarlayın)
2. Account.API projesini aç
3. F5 ile başlat
4. http://localhost:5001 açılacak
```

**C) API Gateway'i başlat:**
```
1. Üçüncü Visual Studio instance
2. ApiGateway projesini aç
3. F5 ile başlat
4. http://localhost:5000 açılacak
```

#### YÖNTEM 2: Multiple Startup Projects (Önerilen)

Visual Studio'da:
```
1. Solution'a sağ tık > Properties
2. Common Properties > Startup Project
3. "Multiple startup projects" seçin
4. Şunları "Start" olarak işaretleyin:
   - ApiGateway
   - Account.API
   - Auth.API
5. Apply > OK
6. F5 ile hepsini birden başlat!
```

#### YÖNTEM 3: Docker Compose ile Hepsini Birden

```powershell
# Tüm servisleri Docker'da çalıştır
docker-compose up -d

# Logları takip et
docker-compose logs -f account-service
```

#### YÖNTEM 4: Terminal'de Manuel (Debug için ideal)

**Terminal 1 - Auth Service:**
```powershell
cd src\Services\Auth\Auth.API
dotnet run
```

**Terminal 2 - Account Service:**
```powershell
cd src\Services\Account\Account.API
dotnet run
```

**Terminal 3 - API Gateway:**
```powershell
cd src\ApiGateway
dotnet run
```

---

## 🧪 Servisleri Test Edin

### 1. Health Check
Tarayıcıdan veya PowerShell'den:

```powershell
# Auth Service
curl http://localhost:5004/health

# Account Service
curl http://localhost:5001/health

# API Gateway
curl http://localhost:5000/health
```

### 2. Swagger UI ile Test

```
Auth Service Swagger:    http://localhost:5004/swagger
Account Service Swagger: http://localhost:5001/swagger
API Gateway:            http://localhost:5000
```

### 3. Postman ile Test

**1. Kullanıcı Kaydı:**
```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test123!",
  "customerId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**2. Login:**
```
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test123!"
}
```

**Response'dan token'ı kopyalayın!**

**3. Hesap Oluştur (JWT ile):**
```
POST http://localhost:5000/api/accounts
Authorization: Bearer YOUR_JWT_TOKEN_HERE
Content-Type: application/json

{
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "accountType": 1,
  "currency": "TRY",
  "dailyWithdrawLimit": 10000
}
```

---

## 🐛 Debug Yapmak

### Visual Studio'da Breakpoint Koyma

```
1. İstediğiniz .cs dosyasını açın (örn: CreateAccountCommandHandler.cs)
2. Satır numarasının soluna tıklayın (kırmızı nokta belirir)
3. F5 ile debug modunda başlatın
4. API'ye istek gönderin
5. Kod breakpoint'te duracak
```

### Örnek Debug Senaryosu:

```csharp
// CreateAccountCommandHandler.cs içinde
public async Task<CreateAccountResponse> Handle(...)
{
    // Buraya breakpoint koy ⭕
    var account = BankAccount.Create(...);
    
    // F10 ile adım adım ilerle
    await _accountRepository.AddAsync(account);
    
    // F11 ile fonksiyonun içine gir
    await _unitOfWork.SaveChangesAsync();
    
    return new CreateAccountResponse { ... };
}
```

### Değişkenleri İzleme:

```
Debug esnasında:
- Locals penceresi: Tüm yerel değişkenleri gösterir
- Watch penceresi: İstediğiniz değişkeni takip edin
- Immediate penceresi: Kod çalıştırın (account.Balance.Amount gibi)
```

---

## 🔍 Infrastructure UI'lara Erişim

Servisler çalışırken:

**RabbitMQ Management:**
```
URL: http://localhost:15672
Username: admin
Password: Admin123!

Queues sekmesinden mesajları görün
Exchanges sekmesinden routing'i kontrol edin
```

**Kibana (Elasticsearch Logs):**
```
URL: http://localhost:5601

1. Management > Stack Management > Index Patterns
2. "account-service-logs-*" pattern oluştur
3. Discover sekmesinden logları görüntüle
```

**Redis Commander (Opsiyonel - eklemek için):**
```powershell
# docker-compose.yml'e ekle:
redis-commander:
  image: rediscommander/redis-commander
  ports:
    - "8081:8081"
  environment:
    - REDIS_HOST=redis
    - REDIS_PASSWORD=Redis123!
```

---

## ⚠️ Sık Karşılaşılan Hatalar ve Çözümleri

### HATA 1: "Docker daemon is not running"
```
❌ Hata: Cannot connect to Docker daemon

✅ Çözüm:
- Docker Desktop'ı başlatın
- Sistem tray'inde Docker icon'unun yeşil olduğundan emin olun
- Komut: docker ps (çalıştığını doğrulayın)
```

### HATA 2: "Port already in use"
```
❌ Hata: Port 5432 is already allocated

✅ Çözüm:
# Portu kullanan servisi bulun
netstat -ano | findstr :5432

# İlgili process'i durdurun
taskkill /PID <process_id> /F

# Veya docker-compose.yml'de portu değiştirin:
ports:
  - "5436:5432"  # 5432 yerine 5436 kullan
```

### HATA 3: "Unable to connect to database"
```
❌ Hata: Npgsql.NpgsqlException: Connection refused

✅ Çözüm:
# PostgreSQL container'ının çalıştığını kontrol edin
docker-compose ps postgres-account

# Healthy olmasını bekleyin (30 saniye)
docker-compose logs postgres-account

# Connection string'i kontrol edin (appsettings.json):
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=AccountDB;Username=admin;Password=Admin123!"
}
```

### HATA 4: "NuGet package restore failed"
```
❌ Hata: Package 'Npgsql.EntityFrameworkCore.PostgreSQL' not found

✅ Çözüm:
# NuGet cache'i temizle
dotnet nuget locals all --clear

# Yeniden restore
dotnet restore
```

### HATA 5: "RabbitMQ connection failed"
```
❌ Hata: RabbitMQ.Client.Exceptions.BrokerUnreachableException

✅ Çözüm:
# RabbitMQ'nun hazır olmasını bekleyin
docker-compose logs rabbitmq | findstr "started"

# Management UI'dan kontrol edin
http://localhost:15672
```

---

## 🎓 Development Workflow

### Tipik Bir Development Günü:

```
1. Sabah:
   - Docker Desktop'ı başlat
   - docker-compose up -d (infrastructure)
   - Visual Studio'yu aç

2. Coding:
   - Feature branch oluştur: git checkout -b feature/new-transaction
   - Kod yaz, test et
   - Breakpoint koy, debug yap

3. Test:
   - Unit testleri çalıştır: dotnet test
   - Postman ile API test et
   - RabbitMQ'da mesajları kontrol et
   - Kibana'da logları incele

4. Commit:
   - git add .
   - git commit -m "Add transaction feature"
   - git push origin feature/new-transaction

5. Akşam:
   - docker-compose down (opsiyonel - volume'lar kalır)
```

---

## 📚 Ek Kaynaklar

### Visual Studio Shortcuts:
```
F5          - Start Debugging
Ctrl+F5     - Start Without Debugging
F9          - Toggle Breakpoint
F10         - Step Over
F11         - Step Into
Shift+F11   - Step Out
Ctrl+K,D    - Format Document
Ctrl+.      - Show Quick Actions
```

### Faydalı Extensions:
```
- ReSharper (code quality)
- CodeMaid (cleanup)
- Docker (container management)
- GitLens (git history)
- REST Client (API testing)
```

---

## 🎯 Sonraki Adımlar

1. ✅ Infrastructure'ı başlattınız
2. ✅ Servisleri Visual Studio'da çalıştırdınız
3. ✅ API'leri test ettiniz
4. 📝 Şimdi yeni feature ekleyebilirsiniz!

**Örnek Yeni Feature:**
- Transaction Service'i tamamlayın
- Customer Service ekleyin
- Notification Service'i geliştirin

---

## 💡 İpuçları

1. **Hot Reload:** .NET 8'de kod değişikliklerini restart olmadan test edebilirsiniz
2. **Multiple Instances:** Her servisi ayrı Visual Studio'da açın (kolaylık için)
3. **Docker Logs:** `docker-compose logs -f service-name` ile canlı log izleyin
4. **Database Viewer:** Visual Studio'da SQL Server Object Explorer ile PostgreSQL'e bağlanın
5. **Memory Profiler:** Visual Studio'nun Diagnostic Tools'unu kullanın

---

## ❓ Sorularınız mı Var?

Sorun yaşarsanız:
1. Docker container'ların healthy olduğunu kontrol edin
2. Port çakışması olup olmadığını kontrol edin
3. Logları inceleyin (Kibana veya docker logs)
4. appsettings.json'da connection string'leri kontrol edin

**İyi kodlamalar! 🚀**
