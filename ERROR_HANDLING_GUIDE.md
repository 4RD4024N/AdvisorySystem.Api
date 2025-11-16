# ?? Error Handling & Troubleshooting Guide

## ?? Common Frontend Errors and Solutions

### 1. 403 Forbidden Errors

#### **Error:**
```javascript
Failed to load resource: the server responded with a status of 403
GET https://localhost:7175/api/statistics/student/summary
```

#### **Root Cause:**
- Role-based authorization mismatch
- User doesn't have required role/permission

#### **Solutions:**

**For Statistics Endpoints:**
- ? `/api/statistics/student/summary` - **No role required** (returns current user's data)
- ?? `/api/statistics/advisor/summary` - **Requires:** Advisor or Admin role
- ?? `/api/statistics/admin/overview` - **Requires:** Admin role

**Check User Role:**
```javascript
// Decode JWT to check roles
const token = localStorage.getItem('token');
const decoded = JSON.parse(atob(token.split('.')[1]));
console.log('User roles:', decoded.role); // Should show role(s)
```

**Fix:**
```javascript
// Only call advisor endpoints if user is advisor/admin
if (userRole === 'Advisor' || userRole === 'Admin') {
  const advisorStats = await api.get('/statistics/advisor/summary');
}
```

---

### 2. 500 Internal Server Error on Notifications

#### **Error:**
```javascript
Failed to load resource: the server responded with a status of 500
GET https://localhost:7175/api/notifications/unread-count
AxiosError: Request failed with status code 500
```

#### **Root Causes:**
1. User ID not found in JWT claims
2. Database connection issue
3. Notification service error

#### **Backend Logs to Check:**
```
[Error] User ID not found in claims. Available claims: ...
[Error] Failed to get unread count: System.NullReferenceException...
```

#### **Solutions:**

**A. Verify Token Structure:**
```javascript
const parseJwt = (token) => {
  try {
    return JSON.parse(atob(token.split('.')[1]));
  } catch (e) {
    console.error('Invalid token', e);
    return null;
  }
};

const decoded = parseJwt(localStorage.getItem('token'));
console.log('Token claims:', decoded);

// Should contain one of: sub, nameidentifier, or name
if (!decoded.sub && !decoded.nameidentifier && !decoded.name) {
  console.error('Token missing user ID claim!');
  // Re-login required
}
```

**B. Handle Error in Frontend:**
```javascript
const fetchNotifications = async () => {
  try {
    const response = await api.get('/notifications');
    setNotifications(response.data);
  } catch (error) {
    if (error.response?.status === 500) {
      console.error('Server error:', error.response.data);
    // Show user-friendly message
      toast.error('Failed to load notifications. Please try again.');
      
    // If persistent, might need to re-login
      if (error.response.data?.details?.includes('User ID not found')) {
        // Token is invalid, force re-login
        localStorage.removeItem('token');
        window.location.href = '/login';
      }
  }
  }
};
```

**C. Backend Enhanced Error Response:**
All 500 errors now include detailed error information:
```json
{
  "error": "Failed to retrieve notifications",
  "details": "User ID not found in claims"
}
```

---

### 3. TypeError: students.map is not a function

#### **Error:**
```javascript
Uncaught TypeError: students.map is not a function
    at Students (Students.jsx:194:23)
```

#### **Root Cause:**
API response structure changed. The response is now an object containing an array:

**Old Response (Expected):**
```json
[
  { "id": "1", "name": "John" },
  { "id": "2", "name": "Jane" }
]
```

**New Response (Actual):**
```json
{
  "students": [
    { "id": "1", "name": "John" },
    { "id": "2", "name": "Jane" }
  ],
  "totalCount": 2,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

#### **Solutions:**

**? Wrong Code:**
```javascript
const Students = () => {
const [students, setStudents] = useState([]);
  
  useEffect(() => {
    api.get('/students').then(response => {
      setStudents(response.data); // response.data is an object, not array!
    });
  }, []);
  
  return (
    <ul>
      {students.map(student => (  // ERROR: students is object, not array
 <li key={student.id}>{student.userName}</li>
      ))}
    </ul>
  );
};
```

**? Correct Code:**
```javascript
const Students = () => {
  const [students, setStudents] = useState([]);
  const [pagination, setPagination] = useState({});
  
  useEffect(() => {
    api.get('/students').then(response => {
      // Destructure the response
      const { students: studentsList, totalCount, page, totalPages } = response.data;
      
      setStudents(studentsList); // Now it's an array
      setPagination({ totalCount, page, totalPages });
    });
  }, []);
  
  return (
    <>
    <div>Total: {pagination.totalCount}</div>
      <ul>
        {students.map(student => (  // Works! students is now an array
       <li key={student.id}>{student.userName}</li>
        ))}
      </ul>
    </>
  );
};
```

**Alternative Fix:**
```javascript
// Option 1: Destructure immediately
const { data: { students, totalCount } } = await api.get('/students');

// Option 2: Access nested property
const response = await api.get('/students');
const studentsArray = response.data.students;
```

---

### 4. CORS Policy Errors

#### **Error:**
```
Access to fetch at 'https://localhost:7175/api/students' from origin 'http://localhost:5173' 
has been blocked by CORS policy: No 'Access-Control-Allow-Origin' header is present
```

#### **Root Cause:**
Frontend port not in allowed CORS origins

#### **Solutions:**

**A. Check Current CORS Configuration:**
```csharp
// Program.cs
builder.Services.AddCors(o =>
{
    o.AddPolicy("frontend", p => p
        .WithOrigins("http://localhost:5173", "http://localhost:3000") 
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});
```

**B. Add Your Frontend Port:**
```csharp
.WithOrigins(
    "http://localhost:5173",  // Vite
    "http://localhost:3000",  // React
    "http://localhost:4200",  // Angular
    "http://localhost:8080"   // Vue CLI
) 
```

**C. Development Only - Allow All (Not for Production!):**
```csharp
// Development only!
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(o =>
    {
        o.AddPolicy("frontend", p => p
            .AllowAnyOrigin()
         .AllowAnyHeader()
            .AllowAnyMethod());
    });
}
```

---

### 5. JWT Token Expiration (401 Unauthorized)

#### **Error:**
```javascript
Response status: 401 Unauthorized
```

#### **Root Cause:**
Tokens expire after 2 hours (120 minutes)

#### **Solutions:**

**A. Check Token Expiration:**
```javascript
const isTokenExpired = () => {
  const token = localStorage.getItem('token');
  if (!token) return true;
  
  try {
    const decoded = JSON.parse(atob(token.split('.')[1]));
    if (!decoded.exp) return true;
    
    // Compare expiration time with current time
    const expirationTime = decoded.exp * 1000; // Convert to milliseconds
    const currentTime = Date.now();
    
    return currentTime >= expirationTime;
  } catch (e) {
  return true;
  }
};

// Check before API calls
if (isTokenExpired()) {
  localStorage.removeItem('token');
  window.location.href = '/login';
}
```

**B. Axios Interceptor (Auto-redirect):**
```javascript
// api/client.js
import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'https://localhost:7175/api'
});

// Response interceptor
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
   // Token expired or invalid
      localStorage.removeItem('token');
window.location.href = '/login';
      
      return Promise.reject(new Error('Session expired. Please login again.'));
    }
    return Promise.reject(error);
  }
);
```

**C. Display Time Until Expiration:**
```javascript
const TokenExpiryWarning = () => {
  const [timeLeft, setTimeLeft] = useState('');
  
  useEffect(() => {
    const interval = setInterval(() => {
      const token = localStorage.getItem('token');
      if (!token) return;
      
      const decoded = JSON.parse(atob(token.split('.')[1]));
      const expiresAt = decoded.exp * 1000;
      const now = Date.now();
      const diff = expiresAt - now;
      
    if (diff <= 0) {
        setTimeLeft('Expired');
  } else {
        const minutes = Math.floor(diff / 60000);
        setTimeLeft(`${minutes} minutes`);
      }
    }, 60000); // Update every minute
    
    return () => clearInterval(interval);
  }, []);
  
  return <div>Session expires in: {timeLeft}</div>;
};
```

---

### 6. File Upload Fails (413 Payload Too Large)

#### **Error:**
```
413 Payload Too Large
```

#### **Root Cause:**
File exceeds 100MB limit

#### **Solutions:**

**A. Client-Side Validation:**
```javascript
const handleFileUpload = async (file) => {
  const MAX_SIZE = 104857600; // 100MB in bytes
  
  if (file.size > MAX_SIZE) {
    const sizeMB = (file.size / 1024 / 1024).toFixed(2);
    alert(`File too large (${sizeMB}MB). Maximum size is 100MB.`);
    return;
  }
  
  // Proceed with upload
  const formData = new FormData();
  formData.append('file', file);
  
  try {
    await api.post(`/documents/${docId}/versions`, formData);
  } catch (error) {
    if (error.response?.status === 413) {
      alert('File is too large. Please compress or split the file.');
    }
  }
};
```

**B. Show File Size:**
```javascript
const formatFileSize = (bytes) => {
  if (bytes === 0) return '0 Bytes';
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
};

// Usage
<input type="file" onChange={(e) => {
const file = e.target.files[0];
  console.log(`File size: ${formatFileSize(file.size)}`);
  
  if (file.size > 104857600) {
    alert('File too large!');
  e.target.value = ''; // Clear selection
  }
}} />
```

---

## ?? Debugging Tips

### 1. Enable Detailed Error Logging

**Backend (appsettings.Development.json):**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "AdvisorySystem.Api": "Debug"
    }
  }
}
```

### 2. Browser DevTools Network Tab

Check:
- Request URL
- Request Headers (Authorization token present?)
- Response Status Code
- Response Body (error details)

### 3. Swagger for Testing

Test endpoints directly:
1. Go to `https://localhost:7175/swagger`
2. Click "Authorize"
3. Paste your token (without "Bearer " prefix)
4. Test endpoints

### 4. Postman Collection

Create a Postman collection for testing:
```json
{
  "info": {
    "name": "Advisory System API"
  },
  "item": [
    {
    "name": "Login",
      "request": {
        "method": "POST",
    "header": [],
 "body": {
     "mode": "raw",
          "raw": "{\n  \"email\": \"stu@local\",\n  \"password\": \"Arda123!\"\n}"
        },
        "url": "https://localhost:7175/api/auth/login"
      }
    }
  ]
}
```

---

## ?? Error Response Formats

All API errors now return consistent formats:

### 400 Bad Request
```json
{
  "error": "Invalid input",
  "validationErrors": {
    "email": ["Email is required"],
    "password": ["Password must be at least 6 characters"]
  }
}
```

### 401 Unauthorized
```json
{
  "error": "Unauthorized",
  "message": "Invalid or expired token"
}
```

### 403 Forbidden
```json
{
  "error": "Forbidden",
  "message": "Insufficient permissions. Required role: Admin"
}
```

### 404 Not Found
```json
{
  "error": "Not Found",
  "message": "Document with ID 123 not found"
}
```

### 500 Internal Server Error
```json
{
  "error": "Failed to retrieve notifications",
  "details": "User ID not found in claims"
}
```

---

## ? Best Practices

### 1. Always Handle Errors
```javascript
const fetchData = async () => {
  try {
    const response = await api.get('/endpoint');
    return response.data;
  } catch (error) {
    if (error.response) {
      // Server responded with error
      console.error('Server error:', error.response.data);
      toast.error(error.response.data.error || 'Something went wrong');
    } else if (error.request) {
      // Request made but no response
      console.error('No response:', error.request);
      toast.error('Server not responding. Please check your connection.');
    } else {
      // Error setting up request
      console.error('Error:', error.message);
      toast.error('Request failed. Please try again.');
    }
    throw error;
  }
};
```

### 2. Validate Before Sending
```javascript
// Validate form data
if (!email || !password) {
  toast.error('Email and password are required');
  return;
}

// Validate file
if (file && file.size > 104857600) {
  toast.error('File too large (max 100MB)');
  return;
}

// Then make request
await api.post('/endpoint', data);
```

### 3. Show Loading States
```javascript
const [loading, setLoading] = useState(false);
const [error, setError] = useState(null);

const fetchData = async () => {
  setLoading(true);
  setError(null);
  
  try {
    const data = await api.get('/endpoint');
    // Handle success
  } catch (err) {
    setError(err.response?.data?.error || 'Failed to load data');
  } finally {
    setLoading(false);
  }
};
```

---

**Last Updated:** 2025-01-06  
**Version:** 1.0.0
