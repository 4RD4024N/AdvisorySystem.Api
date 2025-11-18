# ?? CORS Hatasý Düzeltildi

## ?? Hata

```
Access to XMLHttpRequest at 'https://localhost:7175/api/auth/login' 
from origin 'http://localhost:5174' has been blocked by CORS policy: 
Response to preflight request doesn't pass access control check: 
No 'Access-Control-Allow-Origin' header is present on the requested resource.
```

## ?? Kök Neden

Program.cs'deki CORS politikasýnda **port 5174** tanýmlý deðildi. Sadece 5173 ve 3000 portlarý izin veriliyordu.

**Eski Kod:**
```csharp
builder.Services.AddCors(o =>
{
    o.AddPolicy("frontend", p => p
        .WithOrigins("http://localhost:5173", "http://localhost:3000") 
 .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});
```

## ? Çözüm

Port 5174 CORS politikasýna eklendi.

**Yeni Kod:**
```csharp
builder.Services.AddCors(o =>
{
    o.AddPolicy("frontend", p => p
        .WithOrigins(
   "http://localhost:5173", 
     "http://localhost:5174",  // ? Eklendi
 "http://localhost:3000"
        ) 
        .AllowAnyHeader()
     .AllowAnyMethod()
        .AllowCredentials());
});
```

---

## ?? Uygulama Adýmlarý

### 1. API'yi Yeniden Baþlat

**Visual Studio:**
- `Ctrl + Shift + F5` (Stop)
- `F5` veya `Ctrl + F5` (Start)

**Terminal:**
```bash
# API'yi durdur (Ctrl+C)
# Sonra tekrar baþlat
dotnet run
```

### 2. Frontend'i Test Et

```javascript
// authService.js
const response = await fetch('https://localhost:7175/api/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    email: 'admin@local',
    password: 'Admin123!'
  })
});

// Artýk CORS hatasý vermeyecek ?
```

---

## ?? Ýzin Verilen Portlar

| Port | Framework | Açýklama |
|------|-----------|----------|
| `5173` | Vite (default) | Vite dev server varsayýlan portu |
| `5174` | Vite (alternative) | Ýkinci Vite instance veya çakýþma durumunda |
| `3000` | React/Next.js | Create React App / Next.js varsayýlan portu |

**Not:** Farklý bir port kullanýyorsanýz, Program.cs'e eklemeniz gerekir.

---

## ?? Test

### Browser Console
```javascript
// Baþarýlý request
fetch('https://localhost:7175/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email: 'admin@local', password: 'Admin123!' })
})
.then(r => r.json())
.then(data => console.log('? Success:', data))
.catch(err => console.error('? Error:', err));
```

**Beklenen Sonuç:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-01-19T10:00:00Z",
  "expiresIn": 86400
}
```

### cURL Test
```bash
curl -X POST https://localhost:7175/api/auth/login \
  -H "Content-Type: application/json" \
  -H "Origin: http://localhost:5174" \
  -d '{"email":"admin@local","password":"Admin123!"}'
```

---

## ?? CORS Nedir?

**CORS (Cross-Origin Resource Sharing):** Farklý origin'lerden (domain/port/protocol) gelen isteklere izin verme mekanizmasý.

**Same Origin:** `http://localhost:5174` ? `http://localhost:5174` ?  
**Cross Origin:** `http://localhost:5174` ? `https://localhost:7175` ? (CORS gerekli)

### CORS Ayarlarý

```csharp
.WithOrigins(...)        // Ýzin verilen origin'ler
.AllowAnyHeader()    // Tüm header'lara izin ver
.AllowAnyMethod()   // Tüm HTTP metodlarýna izin ver (GET, POST, PUT, DELETE)
.AllowCredentials()      // Cookie ve credentials'a izin ver
```

---

## ?? Sorun Giderme

### Hala CORS Hatasý Alýyorsanýz

1. **API'yi yeniden baþlattýnýz mý?**
   - Program.cs deðiþiklikleri runtime'da güncellenmez
   - Mutlaka restart gerekir

2. **Doðru portu mu kullanýyorsunuz?**
   ```javascript
   // Frontend package.json veya vite.config.js
   // server: { port: 5174 }
   ```

3. **Browser cache'i temizleyin**
   - `Ctrl + Shift + Del` ? Cache temizle
   - Veya Incognito/Private modda deneyin

4. **HTTPS/HTTP karýþýmý**
   - Frontend: `http://localhost:5174`
   - Backend: `https://localhost:7175`
   - Sorun yok, CORS bunu halleder

5. **Preflight request baþarýsýz mý?**
   ```
   OPTIONS /api/auth/login
   ```
   Browser önce OPTIONS request gönderir, baþarýsýzsa asýl request gitmez.

---

## ?? Production için CORS

**Development:**
```csharp
.WithOrigins("http://localhost:5174", ...)
```

**Production:**
```csharp
.WithOrigins(
    "https://yourdomain.com",
    "https://www.yourdomain.com",
    "https://app.yourdomain.com"
)
```

**Environment-based:**
```csharp
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:5174" };

builder.Services.AddCors(o =>
{
    o.AddPolicy("frontend", p => p
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
      .AllowCredentials());
});
```

**appsettings.json:**
```json
{
  "AllowedOrigins": [
    "http://localhost:5173",
    "http://localhost:5174",
    "https://yourdomain.com"
  ]
}
```

---

## ? Kontrol Listesi

- [x] Port 5174 CORS politikasýna eklendi
- [x] Build baþarýlý
- [ ] API yeniden baþlatýldý
- [ ] Frontend'den test edildi
- [ ] CORS hatasý gitti

---

## ?? Ek Yardým

**Hala sorun varsa kontrol edin:**

1. **Network tab'de request'i inceleyin:**
   - Request Headers ? Origin: `http://localhost:5174`
   - Response Headers ? Access-Control-Allow-Origin: `http://localhost:5174`

2. **Console'da detaylý hata:**
   ```
   Access to XMLHttpRequest at '...' from origin '...' has been blocked by CORS policy
   ```

3. **API loglarý:**
   ```
   info: Microsoft.AspNetCore.Cors.Infrastructure.CorsService[2]
   CORS policy execution successful.
   ```

---

**Hazýrlayan:** Advisory System Team  
**Tarih:** 2025-01-18  
**Durum:** ? Düzeltildi  
**Dosya:** Program.cs  
**Deðiþiklik:** Port 5174 CORS'a eklendi
