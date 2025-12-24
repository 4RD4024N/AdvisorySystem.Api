# ?? Frontend Error Fix Guide

**Date:** 2025-01-06  
**Version:** v3.1.1  
**Error:** `documentService.getAll is not a function`

---

## ?? Problem

Frontend'de þu hata alýnýyor:
```
Veriler yüklenirken bir hata oluþtu: documentService.getAll is not a function
```

---

## ? Solution

### Doðru API Endpoint

Backend'de `GET /api/documents` endpoint'i mevcut ancak **`GetMine`** olarak isimlendirilmiþ.

**Correct URL:**
```
GET https://localhost:7175/api/documents
```

**NOT:** `getAll` yok, `documents` endpoint'i var.

---

## ?? Frontend Service Fix

### Option 1: Update Service Method Name

```javascript
// services/documentService.js

import api from './api';

const documentService = {
  // ? WRONG - getAll doesn't exist
  // getAll: async () => {
  //   return api.get('/documents/all');
  // },

  // ? CORRECT - Use documents endpoint
  getAll: async (params = {}) => {
    const { title, startDate, endDate } = params;
    const queryParams = new URLSearchParams();
    
    if (title) queryParams.append('title', title);
    if (startDate) queryParams.append('startDate', startDate);
    if (endDate) queryParams.append('endDate', endDate);
    
    const url = queryParams.toString() 
      ? `/documents?${queryParams.toString()}`
      : '/documents';
    
    return api.get(url);
  },

  getMine: async (params = {}) => {
    // Same as getAll - both use /documents endpoint
    return documentService.getAll(params);
  },

getById: async (id) => {
    return api.get(`/documents/${id}`);
  },

  create: async (data) => {
    return api.post('/documents', data);
  },

  uploadVersion: async (documentId, formData) => {
    return api.post(`/documents/${documentId}/versions`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
  },

  getVersions: async (documentId) => {
    return api.get(`/documents/${documentId}/versions`);
  },

  download: async (versionId) => {
    return api.get(`/documents/download/${versionId}`, {
      responseType: 'blob'
    });
},

  preview: async (versionId) => {
    return api.get(`/documents/preview/${versionId}`, {
responseType: 'blob'
    });
  },

  getMetadata: async (versionId) => {
    return api.get(`/documents/metadata/${versionId}`);
  }
};

export default documentService;
```

---

### Option 2: Update Component to Use Correct Method

```javascript
// components/Documents.jsx

import { useState, useEffect } from 'react';
import documentService from '../services/documentService';

const Documents = () => {
  const [documents, setDocuments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadDocuments();
  }, []);

  const loadDocuments = async () => {
    try {
      setLoading(true);
      setError('');
      
      // ? Use getMine or getAll
      const response = await documentService.getMine();
    
      // ? Response is already an array from backend
      setDocuments(response.data || []);
    } catch (err) {
      console.error('Failed to load documents:', err);
      setError(err.response?.data?.error || 'Veriler yüklenirken bir hata oluþtu');
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div>Yükleniyor...</div>;
  if (error) return <div className="error">{error}</div>;

  return (
  <div className="documents">
      <h2>Belgelerim</h2>
      
      {documents.length === 0 ? (
        <p>Henüz belge yok.</p>
      ) : (
        <table>
          <thead>
          <tr>
     <th>Baþlýk</th>
              <th>Etiketler</th>
              <th>Tarih</th>
   <th>Versiyon Sayýsý</th>
     </tr>
    </thead>
 <tbody>
            {documents.map(doc => (
   <tr key={doc.id}>
     <td>{doc.title}</td>
         <td>{doc.tags}</td>
                <td>{new Date(doc.createdAt).toLocaleDateString('tr-TR')}</td>
         <td>{doc.versionCount}</td>
  </tr>
            ))}
</tbody>
        </table>
      )}
    </div>
  );
};

export default Documents;
```

---

## ?? API Endpoint Details

### GET /api/documents

**URL:** `https://localhost:7175/api/documents`

**Authorization:** Bearer token required

**Query Parameters (Optional):**
- `title` (string): Search in title
- `startDate` (datetime): Filter from date
- `endDate` (datetime): Filter to date

**Response:**
```json
[
  {
    "id": 1,
    "title": "Thesis Draft",
    "tags": "research,thesis",
    "createdAt": "2024-01-15T10:30:00Z",
    "ownerUserId": "student-id-123",
    "advisorUserId": null,
    "versionCount": 3
  }
]
```

**Response is an ARRAY, not an object!**

---

## ?? Authorization Behavior

### Student
```javascript
// Student sees only their own documents
const response = await api.get('/documents');
// Returns: Documents where ownerUserId = current user ID
```

### Advisor
```javascript
// Advisor sees only their students' documents
const response = await api.get('/documents');
// Returns: Documents where owner.AdvisorId = current advisor ID
```

### Admin
```javascript
// Admin sees all documents
const response = await api.get('/documents');
// Returns: All documents in system
```

---

## ?? Testing

### Test 1: Call Endpoint Directly

```javascript
// Test in browser console
const token = localStorage.getItem('token');

fetch('https://localhost:7175/api/documents', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
})
.then(res => res.json())
.then(data => {
  console.log('Documents:', data);
  console.log('Is Array?', Array.isArray(data));
});
```

**Expected Output:**
```
Documents: [ { id: 1, title: "...", ... } ]
Is Array? true
```

---

### Test 2: Check Service Function

```javascript
// Test service in browser console
import documentService from './services/documentService';

documentService.getMine()
  .then(response => {
    console.log('Service response:', response.data);
  })
  .catch(error => {
    console.error('Service error:', error);
  });
```

---

## ?? Complete Service File Example

```javascript
// services/documentService.js
import api from './api';

const documentService = {
  /**
   * Get documents (behavior depends on user role)
   * - Student: Own documents
   * - Advisor: Students' documents
   * - Admin: All documents
   */
  getMine: async (filters = {}) => {
    try {
      const params = new URLSearchParams();
      
      if (filters.title) {
        params.append('title', filters.title);
      }
      if (filters.startDate) {
        params.append('startDate', filters.startDate);
      }
      if (filters.endDate) {
        params.append('endDate', filters.endDate);
}
      
   const url = params.toString() 
        ? `/documents?${params}` 
   : '/documents';
 
      const response = await api.get(url);
      return response;
    } catch (error) {
      console.error('getMine error:', error);
      throw error;
    }
  },

  // Alias for getMine
  getAll: async (filters = {}) => {
  return documentService.getMine(filters);
  },

  /**
   * Create new document
   * @param {Object} data - { title, tags }
   * @returns {Promise}
   */
  create: async (data) => {
    try {
      const response = await api.post('/documents', data);
      return response;
    } catch (error) {
      console.error('create error:', error);
  throw error;
    }
  },

  /**
   * Upload new version
   * @param {number} documentId
   * @param {FormData} formData - Must contain 'file' and optional 'notes'
   */
  uploadVersion: async (documentId, formData) => {
    try {
      const response = await api.post(
        `/documents/${documentId}/versions`,
    formData,
        {
 headers: {
        'Content-Type': 'multipart/form-data'
          }
        }
    );
      return response;
    } catch (error) {
   console.error('uploadVersion error:', error);
      throw error;
    }
  },

  /**
   * Get versions for document (last 2 versions)
   * @param {number} documentId
   */
  getVersions: async (documentId) => {
    try {
      const response = await api.get(`/documents/${documentId}/versions`);
      return response;
    } catch (error) {
      console.error('getVersions error:', error);
throw error;
    }
  },

  /**
   * Download file
   * @param {number} versionId
   */
  download: async (versionId) => {
  try {
  const response = await api.get(`/documents/download/${versionId}`, {
   responseType: 'blob'
      });
      
      // Create download link
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', response.headers['content-disposition']?.split('filename=')[1] || 'download');
      document.body.appendChild(link);
      link.click();
      link.remove();
   
    return response;
    } catch (error) {
      console.error('download error:', error);
      throw error;
    }
  },

  /**
   * Preview PDF in browser
   * @param {number} versionId
   */
  preview: async (versionId) => {
    try {
      const response = await api.get(`/documents/preview/${versionId}`, {
        responseType: 'blob'
      });
      
      // Open in new tab
      const file = new Blob([response.data], { type: 'application/pdf' });
  const fileURL = URL.createObjectURL(file);
      window.open(fileURL, '_blank');
      
    return response;
    } catch (error) {
      console.error('preview error:', error);
      throw error;
    }
  },

  /**
   * Get metadata for document version
   * @param {number} versionId
   */
  getMetadata: async (versionId) => {
    try {
      const response = await api.get(`/documents/metadata/${versionId}`);
      return response;
    } catch (error) {
      console.error('getMetadata error:', error);
      throw error;
    }
  }
};

export default documentService;
```

---

## ? Summary

**Problem:** Frontend calling `documentService.getAll()` which doesn't match backend endpoint.

**Solution:**
1. ? Backend endpoint is `/api/documents` (not `/api/documents/all`)
2. ? Update frontend service to use correct endpoint
3. ? Response is an array, not `{ documents: [] }`
4. ? Authorization is role-based (Student/Advisor/Admin)

**Files to Update:**
- `services/documentService.js` - Add/fix `getMine()` and `getAll()` methods
- `components/Documents.jsx` - Use `documentService.getMine()`

---

**Status:** ? READY TO IMPLEMENT  
**Action Required:** Update frontend service file

