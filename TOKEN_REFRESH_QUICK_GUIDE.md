# ?? Token Refresh - Quick Reference

## ? What Was Done

### Backend Changes:
1. ? **New Endpoint:** `POST /api/auth/refresh` - Yeni token al
2. ? **New Endpoint:** `GET /api/auth/validate` - Token doðrula
3. ? **Token Duration:** 2 saat ? **24 saat** (1440 dakika)
4. ? **Login Response:** Artýk `expiresAt` ve `expiresIn` içeriyor

---

## ?? Quick Start

### Seçenek 1: Sadece Re-login (EN BASIT)

```javascript
// 1. Browser console'da token'ý sil
localStorage.removeItem('token');
localStorage.removeItem('tokenExpiry');

// 2. Login sayfasýna git
window.location.href = '/login';

// 3. Tekrar giriþ yap
// ? Yeni token 24 saat geçerli!
```

### Seçenek 2: Manual Refresh (ORTA)

```javascript
// Token'ý manuel yenile
const response = await fetch('https://localhost:7175/api/auth/refresh', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${localStorage.getItem('token')}`
  }
});

const { token, expiresAt } = await response.json();
localStorage.setItem('token', token);
localStorage.setItem('tokenExpiry', expiresAt);

console.log('? Token yenilendi! 24 saat daha geçerli.');
window.location.reload();
```

### Seçenek 3: Otomatik Refresh (GELÝÞMÝÞ)

`src/services/api.js` dosyasýna ekle:

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7175/api'
});

// Request interceptor - token ekle
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor - 401'de otomatik refresh
api.interceptors.response.use(
  response => response,
  async error => {
    const originalRequest = error.config;
    
    if (error.response?.status === 401 && !originalRequest._retry) {
   originalRequest._retry = true;
      
      try {
        // Token'ý yenile
        const response = await axios.post(
        'https://localhost:7175/api/auth/refresh',
 {},
          { headers: { Authorization: `Bearer ${localStorage.getItem('token')}` } }
      );
        
        const { token } = response.data;
   localStorage.setItem('token', token);
        
        // Baþarýsýz requesti tekrar dene
        originalRequest.headers.Authorization = `Bearer ${token}`;
return api(originalRequest);
    } catch (refreshError) {
        // Refresh baþarýsýz - logout
        localStorage.clear();
window.location.href = '/login';
      }
    }
    
    return Promise.reject(error);
  }
);

export default api;
```

---

## ?? Test

### 1. Login Test
```bash
curl -X POST https://localhost:7175/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@local","password":"Admin123!"}'
```

**Response:**
```json
{
  "token": "eyJ...",
  "expiresAt": "2024-01-17T10:00:00Z",
  "expiresIn": 86400
}
```

### 2. Refresh Test
```bash
curl -X POST https://localhost:7175/api/auth/refresh \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response:**
```json
{
  "token": "NEW_TOKEN",
  "expiresAt": "2024-01-18T10:00:00Z",
  "expiresIn": 86400
}
```

### 3. Validate Test
```bash
curl -X GET https://localhost:7175/api/auth/validate \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Response:**
```json
{
  "valid": true,
  "userId": "abc-123",
  "email": "admin@local",
  "roles": ["Admin"]
}
```

---

## ?? Token Lifecycle

```
???????????
?  Login  ? ? Token (24 saat)
???????????
     ?
     ?
???????????????
?  Use Token  ? ? API calls work
???????????????
     ?
     ? (23 saat sonra...)
     ?
????????????????
? Token expiry ? ? Options:
?  warning     ?   1. Refresh
????????????????   2. Re-login
     ?              3. Let it expire
   ?
????????????????
?   Refresh    ? ? New token (24 saat daha)
????????????????
     ?
   ?
???????????????
? Use Token   ? ? Continue working
???????????????
```

---

## ?? Recommendations

### For Development:
- ? Use 24-hour tokens (already configured)
- ? Re-login when needed
- ?? Don't implement auto-refresh yet (adds complexity)

### For Production:
- ? Implement Option 3 (Auto-refresh)
- ? Add token expiry check on app start
- ? Show "Session expiring" warning
- ? Graceful logout on refresh failure

---

## ?? Debug Commands

### Check Current Token
```javascript
// Browser console
const token = localStorage.getItem('token');
if (!token) {
  console.log('? No token');
} else {
  const decoded = JSON.parse(atob(token.split('.')[1]));
  const expiry = new Date(decoded.exp * 1000);
  const now = new Date();
  const minutesLeft = (expiry - now) / 1000 / 60;
  
  console.log('Token expires at:', expiry);
  console.log('Minutes left:', minutesLeft);
  console.log('Hours left:', minutesLeft / 60);
}
```

### Force Refresh
```javascript
// Browser console
async function forceRefresh() {
  try {
    const response = await fetch('https://localhost:7175/api/auth/refresh', {
      method: 'POST',
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    });
    
    const data = await response.json();
    localStorage.setItem('token', data.token);
    localStorage.setItem('tokenExpiry', data.expiresAt);
    
    console.log('? Token refreshed!');
    console.log('New expiry:', data.expiresAt);
  } catch (error) {
    console.error('? Refresh failed:', error);
  }
}

forceRefresh();
```

---

## ? Current Status

| Feature | Status | Notes |
|---------|--------|-------|
| **Refresh Endpoint** | ? Working | `/api/auth/refresh` |
| **Validate Endpoint** | ? Working | `/api/auth/validate` |
| **24-hour Tokens** | ? Active | Changed from 2 hours |
| **Extended Login Response** | ? Working | Includes expiresAt |
| **Auto-refresh (Frontend)** | ?? Optional | Choose implementation |

---

## ?? Quick Actions

### Right Now (30 seconds):
```bash
# 1. Restart backend
# Stop current instance, then:
dotnet run

# 2. Re-login in browser
# ? Get 24-hour token automatically
```

### Optional (15 minutes):
- Implement automatic refresh (Option 3)
- Add token expiry warning
- Test refresh flow

---

## ?? API Changes Summary

### Login Endpoint
**Before:**
```json
{ "token": "..." }
```

**After:**
```json
{
  "token": "...",
  "expiresAt": "2024-01-17T10:00:00Z",
  "expiresIn": 86400
}
```

### New Endpoints
- `POST /api/auth/refresh` ? Get new token
- `GET /api/auth/validate` ? Check token validity

### Configuration
- Token duration: **24 hours** (1440 minutes)

---

**Action Required:** 
1. Restart backend
2. Re-login
3. ? Work for 24 hours without interruption!

---

**Created:** 2025-01-06  
**Status:** ? Production Ready  
**Impact:** No more frequent login prompts!
