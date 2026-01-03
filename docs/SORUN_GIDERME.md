# Sýk Karþýlaþýlan Sorunlar ve Çözümleri

---

## 1. CORS Hatasý

**Hata:**
```
Access to fetch at 'https://localhost:7175/api/...' from origin 'http://localhost:5173' has been blocked by CORS policy
```

**Çözüm:**

`Program.cs` dosyasýnda frontend URL'ini ekle:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
       "http://localhost:3000"  // Baþka port kullanýyorsan ekle
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
      .AllowCredentials();
  });
});
```

---

## 2. 401 Unauthorized

**Hata:**
```json
{ "error": "Unauthorized" }
```

**Olasý Sebepler:**
1. Token süresi dolmuþ
2. Token yanlýþ
3. Header'da token yok

**Çözüm:**

Token'ý yenile:
```http
POST /api/auth/refresh
{
  "refreshToken": "senin-refresh-token"
}
```

Veya tekrar giriþ yap.

---

## 3. 403 Forbidden

**Hata:**
```json
{ "error": "Forbidden" }
```

**Sebep:** Yetkisiz iþlem yapýyorsun.

**Örnekler:**
- Admin olarak profil görmeye çalýþýyorsun (Admin bunu yapamaz)
- Advisor baþka danýþmanýn öðrencisine eriþmeye çalýþýyor
- Student baþka öðrencinin verisine eriþmeye çalýþýyor

**Çözüm:** Doðru rol ile giriþ yap.

---

## 4. Schedule Conflict (Çakýþma)

**Hata:**
```json
{
  "error": "Schedule conflict detected",
  "conflictDetails": [...]
}
```

**Sebep:** Ayný saatte baþka ders kayýtlýsýn.

**Çözüm:**
1. Çakýþan dersi býrak
2. Baþka þube seç (örn: A yerine B þubesi)
3. Farklý bir ders seç

---

## 5. Section is Full

**Hata:**
```json
{
  "error": "Section is full",
  "enrolledCount": 50,
  "maxCapacity": 50
}
```

**Sebep:** Þube kapasitesi dolmuþ.

**Çözüm:**
1. Baþka þube seç
2. Danýþmanýnla konuþ

---

## 6. Course Schedule Not Found

**Hata:**
```json
{
  "error": "Course schedule not found"
}
```

**Sebep:** Admin henüz ders programý oluþturmamýþ.

**Çözüm:** Admin'e söyle, schedule generate etsin:
```http
POST /api/schedule/generate/1
Authorization: Bearer {admin_token}
```

---

## 7. Veritabaný Baðlantý Hatasý

**Hata:**
```
Cannot open database "AdvisorySystemDB"
```

**Çözüm:**

1. SQL Server çalýþýyor mu kontrol et
2. Connection string'i kontrol et (`appsettings.json`)
3. Migration'larý uygula:
```bash
dotnet ef database update
```

---

## 8. Migration Hatasý

**Hata:**
```
The entity type 'X' requires a primary key
```

**Çözüm:**

```bash
# Son migration'ý kaldýr
dotnet ef migrations remove

# Yeniden oluþtur
dotnet ef migrations add FixMigration

# Uygula
dotnet ef database update
```

---

## 9. Token Geçersiz

**Hata:**
```
IDX10223: Lifetime validation failed
```

**Sebep:** Token süresi dolmuþ veya geçersiz.

**Çözüm:** Yeni token al (login veya refresh).

---

## 10. Localhost Sertifika Hatasý

**Hata:**
```
SSL certificate problem: unable to get local issuer certificate
```

**Çözüm:**

Development ortamýnda HTTPS sertifikasý güvenilir yap:
```bash
dotnet dev-certs https --trust
```

---

## Hýzlý Debug Checklist

1. [ ] API çalýþýyor mu? (`dotnet run`)
2. [ ] Token geçerli mi?
3. [ ] Doðru endpoint mi?
4. [ ] Doðru HTTP method mu? (GET/POST/PUT/DELETE)
5. [ ] Body doðru formatta mý? (JSON)
6. [ ] Content-Type header var mý?
7. [ ] Authorization header var mý?

---

## Yardýmcý Komutlar

```bash
# Projeyi çalýþtýr
dotnet run

# Build et
dotnet build

# Testleri çalýþtýr
dotnet test

# Migration oluþtur
dotnet ef migrations add MigrationAdi

# Migration uygula
dotnet ef database update

# Son migration'ý kaldýr
dotnet ef migrations remove
```

---

## Swagger UI

Test için Swagger kullan:

```
https://localhost:7175/swagger
```

Buradan tüm endpoint'leri test edebilirsin.
