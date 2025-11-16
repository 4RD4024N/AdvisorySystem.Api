# Storage & Monitoring API Documentation

## ?? Storage Management Endpoints (Admin Only)

All endpoints require `Admin` role.

### 1. Get Storage Information
```http
GET /api/storage/info
Authorization: Bearer {token}
```

**Response:**
```json
{
  "storageType": "Azure Blob Storage",
  "isProduction": true,
  "maxFileSize": "104857600",
  "maxFileSizeMB": 100,
  "uploadPath": "Azure Blob Container"
}
```

---

### 2. Get Storage Statistics
```http
GET /api/storage/statistics
Authorization: Bearer {token}
```

**Response:**
```json
{
  "totalFiles": 150,
  "totalSizeBytes": 52428800,
  "totalSizeMB": 50.0,
"totalSizeGB": 0.05,
  "averageSizeBytes": 349525,
  "averageSizeMB": 0.33,
  "filesByType": [
  {
      "contentType": "application/pdf",
      "count": 85,
   "totalSize": 35651584
    },
    {
  "contentType": "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
      "count": 45,
"totalSize": 12582912
    }
  ]
}
```

---

### 3. List All Files
```http
GET /api/storage/files?prefix=doc_
Authorization: Bearer {token}
```

**Query Parameters:**
- `prefix` (optional): Filter files by prefix

**Response:**
```json
{
  "count": 25,
"files": [
    "https://advisorysystemstorage.blob.core.windows.net/documents/doc_123_thesis.pdf",
    "https://advisorysystemstorage.blob.core.windows.net/documents/doc_456_report.docx"
  ]
}
```

---

### 4. Check File Exists
```http
GET /api/storage/exists?path=https://...blob.../file.pdf
Authorization: Bearer {token}
```

**Response:**
```json
{
  "path": "https://advisorysystemstorage.blob.core.windows.net/documents/file.pdf",
  "exists": true
}
```

---

### 5. Cleanup Orphaned Files
```http
DELETE /api/storage/cleanup-orphaned
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Deleted 3 orphaned files",
  "totalOrphaned": 3,
  "deletedCount": 3,
  "failed": 0
}
```

---

### 6. Get File Metadata
```http
GET /api/storage/metadata/12
Authorization: Bearer {token}
```

**Response:**
```json
{
  "versionId": 12,
  "fileName": "thesis_v2.pdf",
  "contentType": "application/pdf",
  "size": 2048576,
  "sizeMB": 1.95,
  "storagePath": "https://advisorysystemstorage.blob.core.windows.net/documents/doc_123_thesis.pdf",
  "exists": true,
  "uploadedBy": "user-id-123",
  "uploadedAt": "2024-01-15T14:20:00Z",
  "documentId": 5,
  "documentTitle": "Thesis Draft"
}
```

---

## ?? Health & Monitoring Endpoints

### 1. Basic Health Check (Public)
```http
GET /api/health
```

**No authentication required**

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-16T10:30:00Z",
  "version": "1.0.0",
  "environment": "Production"
}
```

---

### 2. Detailed Health Check (Admin)
```http
GET /api/health/detailed
Authorization: Bearer {token}
```

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-16T10:30:00Z",
  "checks": {
    "database": {
  "status": "healthy",
      "canConnect": true,
      "userCount": 45
    },
    "memory": {
      "workingSetMB": 125.5,
      "privateMemoryMB": 150.2
    },
  "configuration": {
      "jwtConfigured": true,
    "storageConfigured": true,
 "corsConfigured": true
    },
    "uptime": {
      "uptimeSeconds": 3600,
      "startTime": "2024-01-16T09:30:00Z"
    }
}
}
```

---

### 3. Database Health Check (Admin)
```http
GET /api/health/database
Authorization: Bearer {token}
```

**Response:**
```json
{
  "status": "healthy",
  "canConnect": true,
  "hasPendingMigrations": false,
  "pendingMigrations": []
}
```

---

### 4. Application Metrics (Admin)
```http
GET /api/health/metrics
Authorization: Bearer {token}
```

**Response:**
```json
{
  "timestamp": "2024-01-16T10:30:00Z",
  "metrics": {
  "users": {
    "total": 45,
      "students": 38
    },
    "documents": {
      "total": 150,
      "withAdvisor": 120
    },
    "versions": {
      "total": 320,
      "totalSizeMB": 450.5
    },
 "submissions": {
      "total": 85,
    "pending": 23,
      "completed": 62
},
    "comments": {
      "total": 450
},
    "notifications": {
      "total": 1250,
      "unread": 150
    }
  }
}
```

---

### 5. System Information (Admin)
```http
GET /api/health/system
Authorization: Bearer {token}
```

**Response:**
```json
{
  "dotnetVersion": "8.0.0",
  "osVersion": "Microsoft Windows NT 10.0.22631.0",
  "machineName": "WEBSERVER01",
  "processorCount": 4,
  "workingSet": {
    "bytes": 131534848,
 "mb": 125.5,
    "gb": 0.12
  },
"uptime": {
    "seconds": 3600,
    "minutes": 60,
    "hours": 1,
    "startTime": "2024-01-16T09:30:00Z"
  }
}
```

---

## ?? Configuration Examples

### Azure Storage Configuration
```json
{
  "Azure": {
    "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=advisorysystemstorage;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net",
    "ContainerName": "documents"
  }
}
```

### Application Insights Configuration
```json
{
  "Azure": {
    "ApplicationInsights": {
      "ConnectionString": "InstrumentationKey=YOUR_KEY;IngestionEndpoint=https://eastus-0.in.applicationinsights.azure.com/"
    }
  }
}
```

---

## ?? Frontend Examples

### Check Storage Type
```javascript
const getStorageInfo = async () => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7175/api/storage/info', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const info = await response.json();
  console.log(`Using ${info.storageType}`);
  return info;
};
```

### Monitor Application Health
```javascript
const checkHealth = async () => {
  const response = await fetch('https://localhost:7175/api/health');
  const health = await response.json();
  
  if (health.status !== 'healthy') {
    alert('System is experiencing issues');
  }
  
  return health;
};

// Detailed health check (admin)
const getDetailedHealth = async () => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7175/api/health/detailed', {
 headers: { 'Authorization': `Bearer ${token}` }
  });
  return await response.json();
};
```

### Get Application Metrics
```javascript
const getMetrics = async () => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7175/api/health/metrics', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const metrics = await response.json();
  
  console.log(`Total Users: ${metrics.metrics.users.total}`);
  console.log(`Total Documents: ${metrics.metrics.documents.total}`);
  
  return metrics;
};
```

### Cleanup Orphaned Files
```javascript
const cleanupOrphanedFiles = async () => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7175/api/storage/cleanup-orphaned', {
    method: 'DELETE',
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const result = await response.json();
  alert(result.message);
  return result;
};
```

---

## ?? Use Cases

### 1. Monitoring Dashboard
```javascript
// Admin dashboard - real-time monitoring
const updateDashboard = async () => {
  const health = await fetch('/api/health/detailed').then(r => r.json());
  const metrics = await fetch('/api/health/metrics').then(r => r.json());
  const storage = await fetch('/api/storage/statistics').then(r => r.json());
  
  // Update UI with stats
  document.getElementById('users').textContent = metrics.metrics.users.total;
  document.getElementById('documents').textContent = metrics.metrics.documents.total;
  document.getElementById('storage').textContent = `${storage.totalSizeGB.toFixed(2)} GB`;
  document.getElementById('health').textContent = health.status;
};

setInterval(updateDashboard, 60000); // Update every minute
```

### 2. Storage Migration Check
```javascript
// Check if using Azure or local storage
const checkStorageMigration = async () => {
  const info = await fetch('/api/storage/info').then(r => r.json());
  
  if (!info.isProduction && window.location.hostname !== 'localhost') {
    console.warn('Production site using local storage. Consider migrating to Azure Blob Storage.');
  }
};
```

### 3. Automated Health Monitoring
```javascript
// Background health check
const monitorHealth = async () => {
  const health = await fetch('/api/health').then(r => r.json());
  
  if (health.status !== 'healthy') {
    // Send alert to admins
    await fetch('/api/notifications', {
      method: 'POST',
      headers: {
      'Authorization': `Bearer ${adminToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
     userId: 'admin-id',
      title: 'System Health Alert',
 message: 'System health check failed. Please investigate.',
     type: 5
      })
    });
  }
};

// Check every 5 minutes
setInterval(monitorHealth, 300000);
```

---

## ?? Azure Deployment Notes

### Enable Application Insights
```bash
# Create Application Insights resource
az monitor app-insights component create \
  --app advisory-system-insights \
  --location eastus \
  --resource-group AdvisorySystemRG

# Get connection string
az monitor app-insights component show \
  --app advisory-system-insights \
  --resource-group AdvisorySystemRG \
  --query connectionString
```

### Configure Azure Blob Storage
```bash
# Create storage account
az storage account create \
  --name advisorysystemstorage \
  --resource-group AdvisorySystemRG \
  --location eastus \
  --sku Standard_LRS

# Get connection string
az storage account show-connection-string \
  --name advisorysystemstorage \
  --resource-group AdvisorySystemRG
```

### Set App Service Configuration
```bash
# Set connection strings in App Service
az webapp config appsettings set \
  --resource-group AdvisorySystemRG \
  --name advisory-system-api \
  --settings \
    Azure__StorageConnectionString="YOUR_STORAGE_CONNECTION_STRING" \
    Azure__ApplicationInsights__ConnectionString="YOUR_INSIGHTS_CONNECTION_STRING"
```

---

## ?? Performance Monitoring

### Key Metrics to Monitor

1. **Response Times**
   - Average: < 200ms
   - P95: < 500ms
   - P99: < 1000ms

2. **Memory Usage**
   - Working Set: < 500MB
   - Private Memory: < 1GB

3. **Database**
   - Connection pool usage
   - Query execution time
   - Failed connections

4. **Storage**
   - Upload success rate
   - Download speed
   - Storage capacity

5. **Errors**
   - 4xx errors (client errors)
 - 5xx errors (server errors)
   - Failed authentication attempts

---

## ?? Alerting Rules

### Recommended Alerts

1. **API Down** - Health check fails for > 5 minutes
2. **High Memory** - Memory usage > 80% for > 10 minutes
3. **Database Unavailable** - Database connection fails
4. **High Error Rate** - 5xx error rate > 5% for > 5 minutes
5. **Storage Full** - Storage usage > 90%

---

## ?? Additional Resources

- **Azure Monitor Documentation:** https://docs.microsoft.com/azure/azure-monitor/
- **Application Insights:** https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview
- **Blob Storage:** https://docs.microsoft.com/azure/storage/blobs/
- **Health Checks:** https://docs.microsoft.com/aspnet/core/host-and-deploy/health-checks

---

**Last Updated:** 2025-01-06  
**API Version:** 1.0.0
