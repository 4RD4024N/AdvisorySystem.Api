# Advisory System API Documentation

**Base URL:** `https://localhost:7175/api`  
**Version:** 1.0  
**.NET:** 8.0

---

## 🔑 Authentication

All protected endpoints require JWT token in header:
```
Authorization: Bearer YOUR_JWT_TOKEN
```

### Default Users
| Email | Password | Role |
|-------|----------|------|
| admin@local | Admin123! | Admin |
| stu@local | Arda123! | Student |

---

## 📋 Endpoints Summary

### Auth
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token

### Documents
- `GET /api/documents` - Get my documents
- `POST /api/documents` - Create document (Student)
- `POST /api/documents/{id}/versions` - Upload file version
- `GET /api/documents/{id}/versions` - Get versions list
- `GET /api/documents/download/{versionId}` - Download file

### Advisors
- `GET /api/advisors` - Get all advisors
- `POST /api/advisors/assign` - Assign advisor to document (Admin/Advisor)

### Comments
- `GET /api/comments/version/{versionId}` - Get comments
- `POST /api/comments` - Create comment
- `DELETE /api/comments/{id}` - Delete comment

### Submissions
- `GET /api/submissions/my` - Get my submissions (Student)
- `POST /api/submissions` - Create submission (Advisor/Admin)
- `PATCH /api/submissions/{id}/status` - Update status (Student)

### Statistics
- `GET /api/statistics/student/summary` - Student stats
- `GET /api/statistics/advisor/summary` - Advisor stats
- `GET /api/statistics/admin/overview` - Admin overview

### Search
- `GET /api/search/documents` - Search documents
- `GET /api/search/tags/popular` - Popular tags

### Notifications
- `GET /api/notifications` - Get my notifications
- `GET /api/notifications/unread-count` - Get unread notification count
- `PATCH /api/notifications/{id}/read` - Mark notification as read
- `PATCH /api/notifications/mark-all-read` - Mark all as read
- `POST /api/notifications` - Create notification (Admin only)

### Debug (Dev Only)
- `GET /api/debug/users` - List all users
- `DELETE /api/debug/users/all` - Delete all users ⚠️
- `GET /api/debug/seedinfo` - Seed info
- `POST /api/debug/token/{email}` - Generate token

---

## 🔐 Authentication Endpoints

### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!",
  "fullName": "John Doe"
}
```

**Response:** `200 OK` (empty)

---

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "stu@local",
  "password": "Arda123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

---

## 📄 Document Endpoints

### Get My Documents
```http
GET /api/documents
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": 1,
    "title": "Thesis Draft",
    "tags": "research,thesis",
    "createdAt": "2024-01-15T10:30:00Z",
    "versionCount": 3
  }
]
```

---

### Create Document
```http
POST /api/documents
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "My Thesis",
  "tags": "research,thesis,software"
}
```

**Response:**
```json
{
  "id": 5
}
```

**Note:** Requires `Student` role

---

### Upload Document Version
```http
POST /api/documents/{id}/versions
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: [binary file, max 100MB]
notes: "Initial draft"
```

**Response:**
```json
{
  "id": 12,
  "versionNo": 2
}
```

---

### Get Document Versions
```http
GET /api/documents/{id}/versions
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
 "id": 12,
    "versionNo": 2,
    "fileName": "thesis_v2.pdf",
    "size": 2048576,
    "createdAt": "2024-01-15T14:20:00Z",
    "notes": "Added references"
  }
]
```

---

### Download File
```http
GET /api/documents/download/{versionId}
Authorization: Bearer {token}
```

**Response:** Binary file stream

---

## 👨‍🏫 Advisor Endpoints

### Get All Advisors
```http
GET /api/advisors
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": "user-id-123",
    "userName": "dr.smith@university.edu",
    "email": "dr.smith@university.edu"
  }
]
```

---

### Assign Advisor
```http
POST /api/advisors/assign
Authorization: Bearer {token}
Content-Type: application/json

{
  "documentId": 5,
  "advisorUserId": "user-id-123"
}
```

**Response:**
```json
{
  "message": "Advisor assigned successfully"
}
```

**Note:** Requires `Admin` or `Advisor` role

---

## 💬 Comment Endpoints

### Get Comments by Version
```http
GET /api/comments/version/{versionId}
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": 8,
    "documentVersionId": 12,
 "authorUserId": "user-id-456",
    "content": "Please add more references",
    "createdAt": "2024-01-16T09:15:00Z"
  }
]
```

---

### Create Comment
```http
POST /api/comments
Authorization: Bearer {token}
Content-Type: application/json

{
  "documentVersionId": 12,
  "content": "Great work! Just minor revisions needed."
}
```

**Response:**
```json
{
  "id": 9,
  "createdAt": "2024-01-16T10:30:00Z"
}
```

---

### Delete Comment
```http
DELETE /api/comments/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Comment deleted"
}
```

**Note:** Only comment author or Admin can delete

---

## 🔔 Notification Endpoints

### Get My Notifications
```http
GET /api/notifications?isRead=false&limit=50
Authorization: Bearer {token}
```

**Query Parameters:**
- `isRead` (optional): Filter by read status (true/false)
- `limit` (optional, default: 50): Maximum number of notifications

**Response:**
```json
{
    "id": "user-id-123",
    "userName": "admin@local",
    "email": "admin@local",
    "emailConfirmed": true,
  "roles": ["Admin"]
  }
]
```

**Notification Types:**
- `0` = DeadlineApproaching
- `1` = NewComment
- `2` = AdvisorAssigned
- `3` = DocumentUploaded
- `4` = SubmissionStatusChanged
- `5` = General

---

### Get Unread Count
```http
GET /api/notifications/unread-count
Authorization: Bearer {token}
```

**Response:**
```json
{
  "unreadCount": 5
}
```

---

### Mark Notification as Read
```http
PATCH /api/notifications/{id}/read
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Notification marked as read"
}
```

---

### Mark All as Read
```http
PATCH /api/notifications/mark-all-read
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "All notifications marked as read"
}
```

---

### Create Notification (Admin Only)
```http
POST /api/notifications
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": "user-id-123",
  "title": "System Maintenance",
  "message": "The system will be down for maintenance on...",
  "type": 5,
  "relatedEntityId": null,
  "relatedEntityType": null
}
```

**Response:**
```json
{
  "message": "Notification created"
}
```

**Note:** Requires `Admin` role

---

## 📅 Submission Endpoints

### Get My Submissions
```http
GET /api/submissions/my
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": 3,
    "studentId": "student-id-789",
    "dueDate": "2024-02-01T23:59:59Z",
 "status": "Pending"
  }
]
```

**Note:** Requires `Student` role

---

### Create Submission
```http
POST /api/submissions
Authorization: Bearer {token}
Content-Type: application/json

{
  "studentId": "student-id-789",
  "dueDate": "2024-02-01T23:59:59Z"
}
```

**Response:**
```json
{
  "id": 4
}
```

**Note:** Requires `Advisor` or `Admin` role

---

### Update Submission Status
```http
PATCH /api/submissions/{id}/status
Authorization: Bearer {token}
Content-Type: application/json

{
  "status": "Completed"
}
```

**Response:**
```json
{
  "status": "Completed"
}
```

**Valid statuses:** `Pending`, `Completed`

---

## 📊 Statistics Endpoints

### Student Summary
```http
GET /api/statistics/student/summary
Authorization: Bearer {token}
```

**Response:**
```json
{
  "totalDocuments": 5,
  "totalVersions": 12,
  "pendingSubmissions": 2,
  "completedSubmissions": 3
}
```

---

### Advisor Summary
```http
GET /api/statistics/advisor/summary
Authorization: Bearer {token}
```

**Response:**
```json
{
  "assignedDocuments": 15,
  "totalComments": 48,
  "studentsCount": 8
}
```

---

### Admin Overview
```http
GET /api/statistics/admin/overview
Authorization: Bearer {token}
```

**Response:**
```json
{
  "totalDocuments": 150,
  "totalVersions": 320,
  "totalSubmissions": 85,
  "totalComments": 450,
  "recentActivity": [
    {
      "id": 45,
      "title": "Research Paper",
      "createdAt": "2024-01-16T15:20:00Z",
      "ownerUserId": "user-id-123"
    }
  ]
}
```

---

## 🔍 Search Endpoints

### Search Documents
```http
GET /api/search/documents?query=thesis&page=1&pageSize=10
Authorization: Bearer {token}
```

**Query Parameters:**
- `query` (optional): Search text
- `tags` (optional): Filter by tags
- `startDate` (optional): ISO 8601 date
- `endDate` (optional): ISO 8601 date
- `page` (optional, default: 1)
- `pageSize` (optional, default: 20)

**Response:**
```json
{
  "totalCount": 25,
  "page": 1,
  "pageSize": 10,
  "totalPages": 3,
  "documents": [
    {
      "id": 1,
      "title": "Thesis Draft",
      "tags": "research,thesis",
      "createdAt": "2024-01-15T10:30:00Z",
      "ownerUserId": "user-id-123",
      "advisorUserId": "user-id-456",
      "versionCount": 3
    }
  ]
}
```

---

### Get Popular Tags
```http
GET /api/search/tags/popular?top=10
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "tag": "thesis",
    "count": 45
  },
  {
    "tag": "research",
    "count": 38
  }
]
```

---

## 📬 Notification Endpoints

### Get My Notifications
```http
GET /api/notifications
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": 1,
    "title": "Document Reviewed",
    "message": "Your document has been reviewed by the advisor.",
    "isRead": false,
    "createdAt": "2024-01-16T08:30:00Z"
  }
]
```

---

### Get Unread Notification Count
```http
GET /api/notifications/unread-count
Authorization: Bearer {token}
```

**Response:**
```json
{
  "count": 2
}
```

---

### Mark Notification as Read
```http
PATCH /api/notifications/{id}/read
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "Notification marked as read"
}
```

---

### Mark All Notifications as Read
```http
PATCH /api/notifications/mark-all-read
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "All notifications marked as read"
}
```

---

### Create Notification (Admin only)
```http
POST /api/notifications
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "New Document Assignment",
  "message": "You have been assigned a new document to review.",
  "userId": "user-id-123"
}
```

**Response:**
```json
{
  "id": 2,
  "createdAt": "2024-01-16T09:00:00Z"
}
```

**Note:** Requires `Admin` role

---

## 🐛 Debug Endpoints (Development Only)

### Get All Users
```http
GET /api/debug/users
```

**Response:**
```json
[
  {
    "id": "user-id-123",
    "userName": "admin@local",
    "email": "admin@local",
    "emailConfirmed": true,
  "roles": ["Admin"]
  }
]
```

---

### Delete All Users ⚠️
```http
DELETE /api/debug/users/all
```

**Response:**
```json
{
  "deletedCount": 15,
  "totalUsers": 15,
  "errors": []
}
```

**⚠️ WARNING:** This deletes all users. Only use in development!

---

### Get Seed Info
```http
GET /api/debug/seedinfo
```

**Response:**
```json
{
  "userCount": 2,
  "roleCount": 3,
  "firstUser": {
    "id": "user-id-123",
    "userName": "admin@local",
    "email": "admin@local"
  }
}
```

---

### Generate Token for User
```http
POST /api/debug/token/stu@local
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

---

## 📝 Common Response Codes

| Code | Description |
|------|-------------|
| 200 | Success |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Missing or invalid token |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found |
| 413 | Payload Too Large - File exceeds 100MB |
| 500 | Internal Server Error |

---

## 🔧 Configuration

### CORS
Frontend must run on: `http://localhost:5173`

### File Upload
- Max file size: 100MB
- Supported: All file types
- Storage: `wwwroot/uploads/`

### JWT Token
- Expiry: 120 minutes (2 hours)
- Algorithm: HMAC-SHA256
- Claims: `sub`, `email`, `name`, `role`

---

## 📚 Quick Examples

### Complete Login Flow
```javascript
// 1. Login
const response = await fetch('https://localhost:7175/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'stu@local',
    password: 'Arda123!'
  })
});
const { token } = await response.json();

// 2. Store token
localStorage.setItem('token', token);

// 3. Use token in subsequent requests
const docs = await fetch('https://localhost:7175/api/documents', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
```

---

### File Upload Example
```javascript
const formData = new FormData();
formData.append('file', fileInput.files[0]);
formData.append('notes', 'First draft');

await fetch(`https://localhost:7175/api/documents/5/versions`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
},
  body: formData
});
```

---

### File Download Example
```javascript
const response = await fetch(
  `https://localhost:7175/api/documents/download/12`,
  {
    headers: { 'Authorization': `Bearer ${token}` }
  }
);

const blob = await response.blob();
const url = window.URL.createObjectURL(blob);
const a = document.createElement('a');
a.href = url;
a.download = 'document.pdf';
a.click();
```

---

## 🚀 Testing with Swagger

Access Swagger UI at: `https://localhost:7175/swagger`

1. Click **Authorize** button (top right)
2. Enter token without "Bearer " prefix
3. Click **Authorize** then **Close**
4. All endpoints now authenticated

---

## 📞 Support

- **Repository:** https://github.com/4RD4024N/AdvisorySystem.Api
- **Issues:** Create issue on GitHub
- **Documentation:** This file

---

**Last Updated:** 2025-01-06  
**Maintained by:** Advisory System Team