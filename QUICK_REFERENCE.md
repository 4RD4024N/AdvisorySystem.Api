# ?? Advisory System API - Quick Reference

**Version:** 3.1.1  
**Last Updated:** 2025-01-06  
**.NET Version:** 8.0

---

## ?? Quick Start

### Base URL
```
https://localhost:7175/api
```

### Authentication
```http
Authorization: Bearer YOUR_JWT_TOKEN
```

**Default Users:**
| Email | Password | Role |
|-------|----------|------|
| admin@local | Admin123! | Admin |
| advisor1@local | Advisor123! | Advisor |
| student1@local | Student123! | Student |

---

## ?? Core Endpoints

### 1. Authentication
```javascript
// Login
POST /api/auth/login
{ "email": "admin@local", "password": "Admin123!" }

// Register
POST /api/auth/register
{ "email": "user@example.com", "password": "Password123!" }
```

### 2. Documents
```javascript
// Get my documents (role-based filtering)
GET /api/documents

// Create document (Student only)
POST /api/documents
{ "title": "My Thesis", "tags": "research" }

// Upload version
POST /api/documents/{id}/versions
FormData: { file, notes }

// Download
GET /api/documents/download/{versionId}

// Preview PDF
GET /api/documents/preview/{versionId}
```

### 3. Submissions (Deadlines)
```javascript
// Get my submissions
GET /api/submissions/my

// Create deadline (Advisor/Admin)
POST /api/submissions
{
  "studentEmail": "student@local",  // ? NEW: Use email or ID
  "dueDate": "2025-02-01T23:59:59Z",
  "notes": "Complete chapter 3"
}
```

### 4. Students (Admin/Advisor)
```javascript
// List students
GET /api/students?search=john

// Get student details
GET /api/students/{id}

// Send notification
POST /api/students/{id}/send-notification
{ "title": "Review", "message": "Check comments", "type": 5 }
```

### 5. Advisors
```javascript
// Assign advisor (Admin only)
POST /api/advisors/assign-to-student
{ "studentId": "...", "advisorId": "..." }

// My advisor (Student)
GET /api/advisors/my-advisor

// My students (Advisor)
GET /api/students/my-students
```

---

## ?? Authorization Rules (v3.1)

### Admin
- ? Full access to everything

### Advisor
- ? **Own students only**
  - View their documents
  - Send notifications
  - Create submissions
  - View submissions
- ? **Cannot access other advisors' students**

### Student
- ? Own documents and submissions
- ? View own advisor
- ? Cannot create submissions

---

## ?? Common Issues & Solutions

### Issue 1: `documentService.getAll is not a function`

**Problem:** Frontend calling wrong method

**Solution:**
```javascript
// ? WRONG
const docs = await api.get('/documents/all');

// ? CORRECT
const docs = await api.get('/documents');
```

**Complete Service:**
```javascript
const documentService = {
  getAll: async () => {
    return api.get('/documents'); // Returns array directly
  },

  getMine: async () => {
    return api.get('/documents'); // Same endpoint
  },

  getVersions: async (docId) => {
    return api.get(`/documents/${docId}/versions`);
  },

  download: async (versionId) => {
    return api.get(`/documents/download/${versionId}`, {
      responseType: 'blob'
    });
  }
};
```

---

### Issue 2: 403 Forbidden (Advisor accessing other students)

**Problem:** Advisor trying to access another advisor's student

**Frontend Fix:**
```javascript
try {
  const response = await api.get(`/students/${studentId}`);
} catch (error) {
  if (error.response?.status === 403) {
    toast.error('You can only access your own students');
  }
}
```

---

### Issue 3: Creating Submission with Email

**Old Way (ID):**
```javascript
await api.post('/submissions', {
  studentId: "abc-123-def-456",  // Need to know ID
  dueDate: "2025-02-01T23:59:59Z"
});
```

**New Way (Email) ?:**
```javascript
await api.post('/submissions', {
  studentEmail: "student@local",// Much easier!
  dueDate: "2025-02-01T23:59:59Z",
  notes: "Complete chapter 3"
});
```

---

## ?? Response Formats

### Documents Response
```javascript
// GET /api/documents
[
  {
    id: 1,
    title: "Thesis",
    tags: "research",
    createdAt: "2024-01-15T10:30:00Z",
    ownerUserId: "student-123",
    versionCount: 3
  }
]
```

### Students Response
```javascript
// GET /api/students
{
  totalCount: 45,
  page: 1,
  pageSize: 20,
  totalPages: 3,
  students: [
{
      id: "student-123",
 userName: "student@local",
      email: "student@local",
   hasAdvisor: true,
      advisor: {
        id: "advisor-456",
        userName: "advisor@local"
      }
    }
  ]
}
```

---

## ?? Frontend Examples

### React: Load Documents
```jsx
const Documents = () => {
  const [documents, setDocuments] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadDocs = async () => {
      try {
        const response = await api.get('/documents');
        setDocuments(response.data); // Already an array
      } catch (error) {
        console.error('Failed to load:', error);
      } finally {
 setLoading(false);
      }
    };
    loadDocs();
  }, []);

  if (loading) return <div>Loading...</div>;

  return (
    <div>
      {documents.map(doc => (
        <div key={doc.id}>
    <h3>{doc.title}</h3>
          <p>{doc.tags}</p>
        </div>
      ))}
    </div>
  );
};
```

### React: Create Deadline
```jsx
const CreateDeadline = () => {
  const [email, setEmail] = useState('');
  const [date, setDate] = useState('');
  const [notes, setNotes] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    try {
      await api.post('/submissions', {
        studentEmail: email,
        dueDate: date,
        notes: notes
      });
      alert('Deadline created!');
    } catch (error) {
      if (error.response?.status === 403) {
        alert('This student is not assigned to you');
      } else {
        alert('Failed to create deadline');
      }
    }
  };

  return (
    <form onSubmit={handleSubmit}>
 <input
        type="email"
        placeholder="Student email"
        value={email}
    onChange={(e) => setEmail(e.target.value)}
      />
      <input
        type="datetime-local"
        value={date}
        onChange={(e) => setDate(e.target.value)}
      />
      <textarea
  placeholder="Notes"
        value={notes}
        onChange={(e) => setNotes(e.target.value)}
      />
      <button type="submit">Create</button>
    </form>
  );
};
```

---

## ?? Backend Info (for debugging)

### File Upload Limits
- **Max Size:** 10MB
- **Allowed Types:** PDF, DOCX, PPTX
- **Middleware:** `FileSizeValidationMiddleware`

### Token Expiry
- **Duration:** 24 hours (1440 minutes)
- **Refresh:** Use `/api/auth/refresh` before expiry

### Database
- **Provider:** SQL Server LocalDB
- **Connection:** `(localdb)\MSSQLLocalDB`
- **Database:** `AdvisorySystemDB`

---

## ?? Changelog

### v3.1.1 (Latest)
- ? Submission creation now accepts **email** instead of just ID
- ? Notes support in submissions
- ? Frontend documentation improved

### v3.1.0
- ? **Advisor permissions restricted** to own students only
- ? Admin-only endpoints added
- ? Authorization matrix updated

### v3.0.0
- ? Advisor assignment to students (not documents)
- ? Simplified API structure
- ? Automatic notifications

---

## ?? Need Help?

### Common Error Codes
- **401:** Token missing or expired ? Re-login
- **403:** Permission denied ? Check user role
- **404:** Resource not found ? Check ID/email
- **413:** File too large ? Max 10MB

### Testing Tools
- **Swagger:** `https://localhost:7175/swagger`
- **Postman:** Import collection (if available)

### Documentation
- Full API Docs: `API_DOCUMENTATION.md`
- Error Fixes: `ERROR_HANDLING_GUIDE.md`

---

**For detailed API reference, see `API_DOCUMENTATION.md`**

