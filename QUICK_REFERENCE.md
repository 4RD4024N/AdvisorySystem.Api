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

### 6. Courses (All Roles)
```javascript
// Get all courses (with filters)
GET /api/courses?categoryId=1&semester=3&isElective=false&search=matematik

// Get course details
GET /api/courses/{id}

// Get categories
GET /api/courses/categories

// Get courses by semester
GET /api/courses/by-semester/3

// Get elective courses
GET /api/courses/electives

// Get my enrolled courses (Student)
GET /api/student-courses/my-program
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

### Issue 3: 403 Forbidden (Advisor cannot comment) ? FIXED

**Problem:** Advisor gets 403 when trying to comment on student's document

**Solution:** Fixed in v3.1.1 backend update

**What Was Wrong:**
- Authorization was using deprecated `document.AdvisorUserId` field
- Now uses `student.AdvisorId` (v3.1 system)

**No Frontend Changes Needed:**
```javascript
// This now works for advisors on their students' documents
await api.post('/comments', {
  documentVersionId: 12,
  content: 'Good work!'
});
```

**Who Can Comment:**
- ? Admin (all documents)
- ? Document owner (own documents)
- ? Advisor (own students' documents only)

---

### Issue 4: Creating Submission with Email

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

## ?? For Advisors

### View My Students
```javascript
// Advisors view students assigned BY ADMIN
const students = await api.get('/advisors/my-students');
console.log(`I have ${students.data.totalStudents} students`);
```

### Rate Student Document
```javascript
// Rate a document version (1-100 score)
await api.post('/ratings', {
  documentVersionId: 12,
  score: 85,
  comments: 'Excellent work! Well-researched.'
});
```

### Update Rating
```javascript
// Rating same version again updates existing rating
await api.post('/ratings', {
  documentVersionId: 12,
  score: 90, // Updated score
  comments: 'Even better after revisions!'
});
```

### View My Ratings
```javascript
// Get all ratings I've given
const advisorId = 'my-advisor-id';
const ratings = await api.get(`/ratings/by-advisor/${advisorId}`);
console.log(`Total ratings: ${ratings.data.totalRatings}`);
console.log(`Average score: ${ratings.data.averageScore}`);
```

### Create Deadline for Student
```javascript
await api.post('/submissions', {
  studentEmail: 'student@local',  // Can use email!
  dueDate: '2025-02-01T23:59:59Z',
  notes: 'Please complete chapter 3'
});
```

### View Student's Documents
```javascript
// All my students' documents
const docs = await api.get('/documents');
```

### Comment on Student's Document
```javascript
await api.post('/comments', {
  documentVersionId: 12,
  content: 'Please revise section 3'
});
```

### Send Notification to Student
```javascript
await api.post(`/students/${studentId}/send-notification`, {
  title: 'Document Review',
  message: 'Your document has been reviewed',
type: 1 // NotificationType.NewComment
});
```

---

## ?? For Students

### View My Documents
```javascript
const docs = await api.get('/documents');
```

### Create Document
```javascript
await api.post('/documents', {
  title: 'My Thesis',
  tags: 'research,thesis,software'
});
```

### Upload Version
```javascript
const formData = new FormData();
formData.append('file', fileInput.files[0]);
formData.append('notes', 'Initial draft');

await api.post(`/documents/${docId}/versions`, formData, {
  headers: { 'Content-Type': 'multipart/form-data' }
});
```

### View My Ratings
```javascript
// Get all ratings received on my documents
const ratings = await api.get('/ratings/my-documents');

// Example response structure
ratings.data.forEach(doc => {
  console.log(`${doc.documentTitle} - Version ${doc.versionNo}`);
  doc.ratings.forEach(rating => {
    console.log(`  Score: ${rating.score}/100`);
    console.log(`Comments: ${rating.comments}`);
  });
});
```

### View Rating for Specific Version
```javascript
// Get ratings for a specific document version
const ratings = await api.get(`/ratings/version/${versionId}`);

if (ratings.data.hasRating) {
  console.log(`Average Score: ${ratings.data.averageScore}`);
  console.log(`Total Ratings: ${ratings.data.ratingCount}`);
}
```

### View My Advisor
```javascript
const response = await api.get('/advisors/my-advisor');
if (response.data.hasAdvisor) {
  console.log('My advisor:', response.data.advisor.userName);
}
```

### View My Submissions
```javascript
const submissions = await api.get('/submissions/my');
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

