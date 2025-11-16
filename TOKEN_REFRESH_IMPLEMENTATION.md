# ?? Token Refresh Implementation Guide

## ? Backend Changes Complete!

### New Endpoints Added:

1. **POST /api/auth/refresh** - Yeni token al (mevcut token ile)
2. **GET /api/auth/validate** - Token doðrula
3. **Token süre uzatýldý:** 2 saat ? **24 saat**

---

## ?? Backend API

### 1. Login (Updated Response)
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@local",
  "password": "Admin123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-17T10:00:00Z",
  "expiresIn": 86400
}
```

---

### 2. Refresh Token (NEW)
```http
POST /api/auth/refresh
Authorization: Bearer {CURRENT_TOKEN}
```

**Response:**
```json
{
  "token": "NEW_TOKEN_HERE",
  "expiresAt": "2024-01-18T10:00:00Z",
  "expiresIn": 86400
}
```

**Features:**
- ? Requires valid (but can be close to expiry) token
- ? Returns new 24-hour token
- ? Works with any role

---

### 3. Validate Token (NEW)
```http
GET /api/auth/validate
Authorization: Bearer {token}
```

**Response (Valid):**
```json
{
  "valid": true,
  "userId": "abc-123",
  "email": "admin@local",
  "roles": ["Admin"]
}
```

**Response (Invalid):**
```json
{
  "valid": false,
  "message": "Token validation failed"
}
```

---

## ?? Frontend Implementation

### Option 1: Manual Refresh (Quick Fix)

Create `src/utils/tokenRefresh.js`:

```javascript
import api from '../services/api';

export const refreshToken = async () => {
  try {
    const response = await api.post('/auth/refresh');
    const { token, expiresAt } = response.data;
    
    // Save new token
    localStorage.setItem('token', token);
    localStorage.setItem('tokenExpiry', expiresAt);
    
console.log('? Token refreshed successfully');
    return token;
  } catch (error) {
    console.error('? Token refresh failed:', error);
    // Force logout
    localStorage.removeItem('token');
    localStorage.removeItem('tokenExpiry');
    window.location.href = '/login';
    throw error;
  }
};

export const isTokenExpiringSoon = () => {
  const expiry = localStorage.getItem('tokenExpiry');
  if (!expiry) return true;
  
  const expiryDate = new Date(expiry);
  const now = new Date();
  const minutesUntilExpiry = (expiryDate - now) / 1000 / 60;
  
  // Refresh if less than 30 minutes remaining
  return minutesUntilExpiry < 30;
};
```

**Usage:**
```javascript
// In any component
import { refreshToken, isTokenExpiringSoon } from './utils/tokenRefresh';

// Check on page load
useEffect(() => {
  if (isTokenExpiringSoon()) {
    refreshToken();
  }
}, []);
```

---

### Option 2: Automatic Refresh (Recommended)

Update `src/services/api.js`:

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7175/api',
  headers: {
    'Content-Type': 'application/json'
  }
});

let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
  failedQueue.forEach(prom => {
    if (error) {
    prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  
  failedQueue = [];
};

// Request interceptor
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
 return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // If 401 and not already retrying
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        // Wait for refresh to complete
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
 })
          .then(token => {
       originalRequest.headers.Authorization = `Bearer ${token}`;
            return api(originalRequest);
          })
   .catch(err => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
    // Try to refresh token
        const response = await axios.post(
       'https://localhost:7175/api/auth/refresh',
 {},
   {
            headers: {
      Authorization: `Bearer ${localStorage.getItem('token')}`
}
     }
        );

    const { token, expiresAt } = response.data;

        // Save new token
        localStorage.setItem('token', token);
        localStorage.setItem('tokenExpiry', expiresAt);

 // Update default header
        api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
        
        // Process queued requests
processQueue(null, token);
        
        // Retry original request
        originalRequest.headers.Authorization = `Bearer ${token}`;
        return api(originalRequest);
      } catch (refreshError) {
        // Refresh failed - logout
    processQueue(refreshError, null);
        localStorage.removeItem('token');
  localStorage.removeItem('tokenExpiry');
 window.location.href = '/login?reason=session_expired';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export default api;
```

**Features:**
- ? Automatic token refresh on 401
- ? Queues failed requests during refresh
- ? Retries all failed requests with new token
- ? Falls back to login if refresh fails

---

### Option 3: Periodic Background Refresh

Add to `src/App.jsx`:

```javascript
import { useEffect } from 'react';
import api from './services/api';

function App() {
  useEffect(() => {
    // Check and refresh token every 10 minutes
    const refreshInterval = setInterval(async () => {
 const expiry = localStorage.getItem('tokenExpiry');
      if (!expiry) return;
   
      const expiryDate = new Date(expiry);
      const now = new Date();
    const minutesUntilExpiry = (expiryDate - now) / 1000 / 60;
      
      // Refresh if less than 30 minutes remaining
      if (minutesUntilExpiry < 30 && minutesUntilExpiry > 0) {
    try {
          console.log('?? Auto-refreshing token...');
        const response = await api.post('/auth/refresh');
      const { token, expiresAt } = response.data;
          
          localStorage.setItem('token', token);
        localStorage.setItem('tokenExpiry', expiresAt);
          console.log('? Token auto-refreshed');
        } catch (error) {
          console.error('? Auto-refresh failed:', error);
        }
      }
    }, 10 * 60 * 1000); // Every 10 minutes

return () => clearInterval(refreshInterval);
  }, []);

  return (
    // Your app JSX
  );
}
```

---

## ?? Testing

### Test 1: Login and Save Expiry
```javascript
const response = await api.post('/auth/login', {
  email: 'admin@local',
password: 'Admin123!'
});

const { token, expiresAt, expiresIn } = response.data;
localStorage.setItem('token', token);
localStorage.setItem('tokenExpiry', expiresAt);

console.log('Token expires at:', new Date(expiresAt));
console.log('Token expires in:', expiresIn / 3600, 'hours');
```

### Test 2: Manual Refresh
```javascript
// In browser console
const response = await fetch('https://localhost:7175/api/auth/refresh', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${localStorage.getItem('token')}`
  }
});

const data = await response.json();
console.log('New token:', data);

// Save it
localStorage.setItem('token', data.token);
localStorage.setItem('tokenExpiry', data.expiresAt);
```

### Test 3: Validate Token
```javascript
const response = await fetch('https://localhost:7175/api/auth/validate', {
  headers: {
    'Authorization': `Bearer ${localStorage.getItem('token')}`
  }
});

const data = await response.json();
console.log('Token validation:', data);
// Should show: { valid: true, userId: "...", email: "...", roles: [...] }
```

---

## ?? Summary

| Feature | Status | Details |
|---------|--------|---------|
| **POST /auth/refresh** | ? Added | Get new token using current token |
| **GET /auth/validate** | ? Added | Check if token is valid |
| **Token duration** | ? Extended | 2 hours ? 24 hours |
| **Login response** | ? Updated | Now includes expiresAt & expiresIn |
| **Auto-refresh** | ?? Frontend | Implement one of 3 options |

---

## ?? Quick Start (Recommended Steps)

### Backend: ? DONE
1. ? Refresh endpoint added
2. ? Validate endpoint added
3. ? Token duration extended to 24 hours
4. ? Build successful

### Frontend: Your Choice

**Quick Fix (5 minutes):**
- Just re-login and use 24-hour tokens
- No code changes needed

**Better Solution (15 minutes):**
- Implement Option 2 (Automatic refresh with axios interceptor)
- Add to `src/services/api.js`

**Best Solution (30 minutes):**
- Combine Option 2 + Option 3
- Automatic refresh on 401 + periodic background check

---

## ?? Debugging

### Check Token Expiry
```javascript
// Browser console
const token = localStorage.getItem('token');
const decoded = JSON.parse(atob(token.split('.')[1]));
console.log('Expires at:', new Date(decoded.exp * 1000));

const now = new Date();
const expiry = new Date(decoded.exp * 1000);
const minutesLeft = (expiry - now) / 1000 / 60;
console.log('Minutes until expiry:', minutesLeft);
```

### Force Refresh Test
```javascript
// Manually trigger refresh
fetch('https://localhost:7175/api/auth/refresh', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${localStorage.getItem('token')}`
  }
})
  .then(r => r.json())
  .then(data => {
    console.log('New token received:', data);
    localStorage.setItem('token', data.token);
    window.location.reload();
});
```

---

## ? Status

**Backend:** ? Complete and tested  
**Frontend:** ?? Choose implementation method  
**Token Duration:** ? 24 hours  
**Auto-refresh API:** ? Available at `/api/auth/refresh`

---

**Next Steps:**
1. Restart backend (to load new 24-hour token duration)
2. Re-login to get new 24-hour token
3. (Optional) Implement frontend auto-refresh
4. Test monitoring page - should work for 24 hours!

---

**Created:** 2025-01-06  
**Backend Status:** ? Production Ready  
**Frontend Status:** ?? Implementation Optional (24-hour tokens already enough)
