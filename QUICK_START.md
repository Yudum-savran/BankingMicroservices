# 🚀 HIZLI BAŞLANGIÇ REHBERİ

## ✅ Gereksinimler

- Visual Studio 2022
- .NET 8.0 SDK
- Docker Desktop (Infrastructure için)

---

## 🎯 3 ADIMDA ÇALIŞTIRIN

### ADIM 1: Docker Infrastructure'ı Başlatın

PowerShell açın:

```powershell
cd "yol\banking-complete-solution"

# Infrastructure servislerini başlat
docker compose up -d postgres-account postgres-auth redis rabbitmq elasticsearch kibana
```

**⏳ 30 saniye bekleyin!**

Kontrol edin:
```powershell
docker compose ps
```

Hepsi "Up (healthy)" olmalı!

---

### ADIM 2: Visual Studio'da Solution'ı Açın

1. **BankingMicroservices.sln** dosyasına çift tıklayın
2. Visual Studio 2022 açılacak
3. Solution yüklensin (30 saniye)

---

### ADIM 3: Multiple Startup Projects Ayarlayın ve Çalıştırın

1. Solution'a **sağ tık** > **Properties**
2. **Multiple startup projects** seçin
3. Şunları **Start** yapın:
   - ✅ **Account.API**
   - ✅ **Auth.API**
   - ✅ **ApiGateway**
4. **OK** tıklayın
5. **F5** tuşuna basın!

**🎉 3 tarayıcı penceresi açılacak:**
```
http://localhost:5001/swagger - Account Service
http://localhost:5004/swagger - Auth Service  
http://localhost:5000        - API Gateway
```

---

## 🧪 İLK TEST - Kullanıcı Kaydı

**Swagger'da (http://localhost:5004/swagger):**

1. **POST /api/auth/register** açın
2. "Try it out" tıklayın
3. JSON:
```json
{
  "email": "test@example.com",
  "password": "Test123!",
  "customerId": "550e8400-e29b-41d4-a716-446655440000"
}
```
4. **Execute** tıklayın
5. ✅ **201 Created** almalısınız!

---

## 🔧 Sorun Giderme

### "Cannot connect to database"
```powershell
# PostgreSQL çalışıyor mu kontrol edin
docker compose ps postgres-account
```

### "Cannot restore NuGet packages"
```
Visual Studio'da:
Tools > NuGet Package Manager > Package Manager Console
dotnet restore
```

### Port çakışması
```
Visual Studio'da projelerin Properties > launchSettings.json'da
portları değiştirin (5001, 5004, 5000)
```

---

## 📚 Detaylı Rehberler

- **VISUAL_STUDIO_GUIDE.md** - Tam Visual Studio rehberi
- **USAGE_GUIDE.md** - API kullanım örnekleri
- **ARCHITECTURE.md** - Mimari detayları

---

## 🎯 Sonraki Adımlar

1. Login olun ve JWT token alın
2. Hesap oluşturun
3. Para yatırın/çekin
4. RabbitMQ'yu kontrol edin: http://localhost:15672
5. Kibana'da logları görün: http://localhost:5601

**İyi kodlamalar! 🚀**
