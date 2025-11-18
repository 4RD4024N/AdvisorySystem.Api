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

### JWT Token
- Expiry: 1440 minutes (24 hours)
- Algorithm: HMAC-SHA256
- Claims: `sub`, `email`, `name`, `role`, `nameidentifier`, `uid`
- Refresh: Available via `/api/auth/refresh` endpoint
- Validate: Check validity with `/api/auth/validate` endpoint

**Token Lifecycle:**
1. Login → Get 24-hour token
2. Use token for API calls
3. (Optional) Refresh before expiry with `/api/auth/refresh`
4. (Optional) Validate with `/api/auth/validate`
5. Logout or let it expire

**Best Practices:**
- Store token in localStorage
- Store expiresAt timestamp
- Refresh token 30 minutes before expiry
- Handle 401 errors by refreshing or re-login

---

## 📋 Endpoints Summary

### Auth
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token
- `POST /api/auth/refresh` - Refresh JWT token (NEW)
- `GET /api/auth/validate` - Validate JWT token (NEW)

### Documents
- `GET /api/documents` - Get my documents
- `POST /api/documents` - Create document (Student)
- `POST /api/documents/{id}/versions` - Upload file version
- `GET /api/documents/{id}/versions` - Get versions list
- `GET /api/documents/download/{versionId}` - Download file
- `GET /api/documents/preview/{versionId}` - Preview PDF file (NEW)
- `GET /api/documents/metadata/{versionId}` - Get file metadata (NEW)

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

### Students (Admin/Advisor)
- `GET /api/students` - Get all students with search
- `GET /api/students/{id}` - Get student details
- `POST /api/students/{id}/send-notification` - Send notification to student
- `POST /api/students/send-bulk-notification` - Send notification to multiple students
- `POST /api/students/send-notification-to-all` - Send notification to all students
- `GET /api/students/without-advisor` - Get students without advisor
- `GET /api/students/with-pending-submissions` - Get students with pending submissions

### Storage Management (Admin)
- `GET /api/storage/info` - Get storage configuration info
- `GET /api/storage/statistics` - Get storage statistics
- `GET /api/storage/files` - List all files
- `GET /api/storage/exists` - Check if file exists
- `DELETE /api/storage/cleanup-orphaned` - Clean up orphaned files
- `GET /api/storage/metadata/{versionId}` - Get file metadata

### Health & Monitoring
- `GET /api/health` - Basic health check (public)
- `GET /api/health/detailed` - Detailed health check (Admin)
- `GET /api/health/database` - Database connectivity check (Admin)
- `GET /api/health/metrics` - Application metrics (Admin)
- `GET /api/health/system` - System information (Admin)

### Search
- `GET /api/search/documents` - Search documents
- `GET /api/search/tags/popular` - Popular tags

### Notifications
- `GET /api/notifications` - Get my notifications
- `GET /api/notifications/unread-count` - Get unread notification count
- `PATCH /api/notifications/{id}/read` - Mark notification as read
- `PATCH /api/notifications/mark-all-read` - Mark all as read
- `POST /api/notifications` - Create notification (Admin only)
- `GET /api/notifications/test-claims` - Test JWT claims (Debug)

### Debug (Dev Only)
- `GET /api/debug/users` - List all users
- `DELETE /api/debug/users/all` - Delete all users ⚠️
- `GET /api/debug/seedinfo` - Seed info
- `POST /api/debug/token/{email}` - Generate token
- `GET /api/debug/users-without-roles` - List users without roles (NEW)
- `POST /api/debug/fix-missing-roles` - Assign Student role to users without roles (NEW)

### Student Profile (NEW)
- `GET /api/studentprofile/me` - Get my profile
- `POST /api/studentprofile` - Create or update profile
- `GET /api/studentprofile/{studentId}` - Get profile by student ID (Admin/Advisor)
- `GET /api/studentprofile/check-prerequisites` - Check if prerequisites are met

### Course Requirements (NEW)
- `GET /api/course/requirements` - Get all course requirements
- `POST /api/course/requirements` - Add course requirement (Admin)
- `GET /api/course/my-courses` - Get my completed courses
- `POST /api/course/my-courses` - Add course to my record
- `PATCH /api/course/my-courses/{id}` - Update course completion

### Document Ratings (NEW)
- `POST /api/ratings` - Create or update rating (Advisor/Admin)
- `GET /api/ratings/version/{versionId}` - Get ratings for document version
- `GET /api/ratings/by-advisor/{advisorId}` - Get all ratings by advisor
- `GET /api/ratings/my-documents` - Get ratings for my documents (Student)
- `DELETE /api/ratings/{id}` - Delete rating

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

**Response:** `200 OK`

```json
{
  "message": "Registration successful",
  "userId": "abc-123-def-456"
}
```

**Note:** 
- Newly registered users are automatically assigned the **Student** role
- If role assignment fails, user is still created but a warning is returned
- Email is used as username

**Error Response (Role Assignment Failed):**
```json
{
  "message": "User created but role assignment failed. Please contact administrator.",
  "userId": "abc-123-def-456",
  "warning": "Student role not assigned"
}
```

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
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-17T10:00:00Z",
  "expiresIn": 86400
}
```

**Note:** Token is valid for 24 hours (1440 minutes).

---

### Refresh Token (NEW)
```http
POST /api/auth/refresh
Authorization: Bearer {current_token}
```

**Purpose:** Get a new token before the current one expires.

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-18T10:00:00Z",
  "expiresIn": 86400
}
```

**Note:** 
- Requires a valid (but can be expiring) token
- Returns a new 24-hour token
- Use this to prevent session interruption

**Example:**
```javascript
// Refresh token before it expires
const response = await api.post('/auth/refresh');
localStorage.setItem('token', response.data.token);
localStorage.setItem('tokenExpiry', response.data.expiresAt);
```

---

### Validate Token (NEW)
```http
GET /api/auth/validate
Authorization: Bearer {token}
```

**Purpose:** Check if current token is valid and get user info.

**Response (Valid):**
```json
{
  "valid": true,
  "userId": "abc-123-def-456",
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

**Use Cases:**
- Check token validity on app startup
- Verify user session before critical operations
- Debug authentication issues

---

## 📄 Document Endpoints

### Get My Documents
```http
GET /api/documents?title=tez&startDate=2024-01-01&endDate=2024-12-31
Authorization: Bearer {token}
```

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `title` | string | No | Search in document title (partial match) |
| `startDate` | datetime | No | Filter from this date (ISO 8601) |
| `endDate` | datetime | No | Filter until this date (ISO 8601) |

**Authorization & Filtering:**
- **Admin**: Can see all documents
- **Advisor**: Can see only their students' documents (where `advisorUserId` matches)
- **Student**: Can see only their own documents (where `ownerUserId` matches)

**Response:**
```json
[
  {
    "id": 1,
"title": "Thesis Draft",
    "tags": "research,thesis",
    "createdAt": "2024-01-15T10:30:00Z",
    "ownerUserId": "student-id-123",
    "advisorUserId": "advisor-id-456",
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

file: [binary file, max 10MB]
notes: "Initial draft"
```

**File Upload Rules:**
- **Maximum Size**: 10MB (10,485,760 bytes)
- **Allowed Types**: PDF, DOCX, PPTX only
- **Validation**: Automatic via middleware

**Response:**
```json
{
  "id": 12,
  "versionNo": 2
}
```

**Error Responses:**

**413 - File Too Large:**
```json
{
"error": "File size exceeds limit",
  "message": "File 'large_doc.pdf' is 15.50MB. Maximum allowed size is 10MB.",
  "maxSizeMB": 10,
  "fileSizeMB": 15.5
}
```

**400 - Invalid File Type:**
```json
{
  "error": "Invalid file type",
  "message": "File type '.xlsx' is not allowed. Only PDF, DOCX, and PPTX files are accepted.",
  "allowedTypes": [".pdf", ".docx", ".pptx"],
  "providedType": ".xlsx"
}
```

**400 - Invalid Content Type:**
```json
{
  "error": "Invalid content type",
  "message": "Content type 'application/vnd.ms-excel' is not allowed.",
  "allowedTypes": ["PDF", "DOCX", "PPTX"]
}
```

---

### Get Document Versions
```http
GET /api/documents/{id}/versions
Authorization: Bearer {token}
```

**Authorization:**
- Document owner
- Assigned advisor
- Admin

**Version Limit:** Returns only the **last 2 versions** (current + 1 previous)

**Response:**
```json
[
  {
    "id": 12,
    "versionNo": 2,
    "fileName": "thesis_v2.pdf",
    "size": 2048576,
    "sizeInMB": 2.0,
    "createdAt": "2024-01-15T14:20:00Z",
"notes": "Added references",
    "contentType": "application/pdf"
  },
  {
    "id": 11,
    "versionNo": 1,
  "fileName": "thesis_v1.pdf",
    "size": 1572864,
    "sizeInMB": 1.5,
  "createdAt": "2024-01-10T10:00:00Z",
    "notes": "First draft",
"contentType": "application/pdf"
  }
]
```

**Note:** Only the most recent 2 versions are visible to users. Older versions are kept in database but not displayed.

---

### Download File
```http
GET /api/documents/download/{versionId}
Authorization: Bearer {token}
```

**Response:** Binary file stream

---

### Preview Document (NEW)
```http
GET /api/documents/preview/{versionId}
Authorization: Bearer {token}
```

**Purpose:** Display PDF in browser without downloading

**Authorization:** Document owner, assigned advisor, or Admin

**Response:** PDF file stream (inline disposition)

**Error Responses:**

400 Bad Request (Non-PDF file):
```json
{
  "error": "Only PDF files can be previewed",
  "contentType": "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
  "message": "Please download the file to view it"
}
```

**Frontend Example:**
```html
<!-- Embed PDF in iframe -->
<iframe 
  src="https://localhost:7175/api/documents/preview/12?token=YOUR_TOKEN" 
  width="100%" 
  height="600px">
</iframe>

<!-- Or use PDF.js -->
<div id="pdf-viewer"></div>
<script src="https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.min.js"></script>
<script>
const url = 'https://localhost:7175/api/documents/preview/12';
pdfjsLib.getDocument({
  url: url,
  httpHeaders: { 'Authorization': `Bearer ${token}` }
}).promise.then(pdf => {
  // Render PDF
});
</script>
```

---

### Get Document Metadata
```http
GET /api/documents/metadata/{versionId}
Authorization: Bearer {token}
```

**Purpose:** Get file information without downloading

**Response:**
```json
{
  "id": 12,
  "fileName": "thesis_final.pdf",
  "contentType": "application/pdf",
  "size": 2048576,
  "sizeFormatted": "2 MB",
  "versionNo": 3,
  "createdAt": "2024-01-15T14:20:00Z",
  "notes": "Final version with corrections",
  "documentId": 5,
  "documentTitle": "My Thesis",
  "isPdf": true,
  "canPreview": true,
  "downloadUrl": "/api/documents/download/12",
  "previewUrl": "/api/documents/preview/12"
}
```

**Non-PDF File Response:**
```json
{
  "id": 13,
  "fileName": "data.xlsx",
  "contentType": "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
  "size": 512000,
  "sizeFormatted": "500 KB",
  "isPdf": false,
  "canPreview": false,
  "downloadUrl": "/api/documents/download/13",
  "previewUrl": null
}
```

---

## 🎓 Student Profile Endpoints (NEW)

### Get My Profile
```http
GET /api/studentprofile/me
Authorization: Bearer {token}
```

**Response:**
```json
{
  "id": 1,
  "userId": "student-id-123",
  "userName": "john.doe@university.edu",
  "studentNumber": "20240001",
  "department": "Computer Science",
  "gpa": 3.75,
  "completedCredits": 120,
  "enrollmentDate": "2020-09-01T00:00:00Z",
  "meetsPrerequisites": true,
  "createdAt": "2024-01-10T09:00:00Z",
  "updatedAt": "2024-01-15T14:30:00Z"
}
```

**404 Not Found:**
```json
{
  "error": "Profile not found"
}
```

---

### Create or Update Profile
```http
POST /api/studentprofile
Authorization: Bearer {token}
Content-Type: application/json

{
  "studentNumber": "20240001",
  "department": "Computer Science",
  "gpa": 3.75,
  "completedCredits": 120,
  "enrollmentDate": "2020-09-01T00:00:00Z"
}
```

**Response:**
```json
{
  "message": "Profile saved successfully",
  "id": 1,
  "userId": "student-id-123"
}
```

**Note:** All fields are optional. Updates existing profile or creates new one.

---

### Get Profile by Student ID (Admin/Advisor)
```http
GET /api/studentprofile/{studentId}
Authorization: Bearer {token}
```

**Authorization:** Requires `Admin` or `Advisor` role

**Response:**
```json
{
  "id": 1,
  "userId": "student-id-123",
  "userName": "john.doe@university.edu",
  "email": "john.doe@university.edu",
  "studentNumber": "20240001",
  "department": "Computer Science",
  "gpa": 3.75,
  "completedCredits": 120,
  "enrollmentDate": "2020-09-01T00:00:00Z",
  "meetsPrerequisites": true,
  "createdAt": "2024-01-10T09:00:00Z",
  "updatedAt": "2024-01-15T14:30:00Z"
}
```

---

### Check Prerequisites
```http
GET /api/studentprofile/check-prerequisites
Authorization: Bearer {token}
```

**Purpose:** Check if student meets course and credit requirements for project eligibility

**Response (Meets Requirements):**
```json
{
  "meetsPrerequisites": true,
  "completedCredits": 120,
  "requiredCredits": 90,
  "completedCoursesCount": 15,
  "requiredCoursesCount": 12,
  "missingCredits": 0,
  "gpa": 3.75,
  "message": "✅ You meet all prerequisites!"
}
```

**Response (Missing Requirements):**
```json
{
  "meetsPrerequisites": false,
  "completedCredits": 75,
  "requiredCredits": 90,
  "completedCoursesCount": 10,
  "requiredCoursesCount": 12,
  "missingCredits": 15,
  "gpa": 3.45,
  "message": "❌ Missing 15 credits"
}
```

---

## 📚 Course Requirements Endpoints (NEW)

### Get All Course Requirements
```http
GET /api/course/requirements
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": 1,
    "courseName": "Data Structures",
    "courseCode": "CS201",
    "credits": 6,
    "isRequired": true,
    "description": "Fundamental data structures and algorithms"
  },
  {
    "id": 2,
    "courseName": "Database Systems",
    "courseCode": "CS301",
    "credits": 6,
    "isRequired": true,
    "description": "Relational database design and SQL"
  }
]
```

---

### Add Course Requirement (Admin)
```http
POST /api/course/requirements
Authorization: Bearer {token}
Content-Type: application/json

{
  "courseName": "Software Engineering",
  "courseCode": "CS401",
"credits": 8,
  "isRequired": true,
  "description": "Software development methodologies and practices"
}
```

**Authorization:** Requires `Admin` role

**Response:**
```json
{
  "message": "Course requirement added",
  "id": 3
}
```

---

### Get My Courses
```http
GET /api/course/my-courses
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": 1,
    "courseRequirementId": 1,
    "courseName": "Data Structures",
    "courseCode": "CS201",
    "credits": 6,
    "isCompleted": true,
    "grade": 85.5,
    "completionDate": "2023-06-15T00:00:00Z"
  },
  {
    "id": 2,
    "courseRequirementId": 2,
    "courseName": "Database Systems",
    "courseCode": "CS301",
"credits": 6,
    "isCompleted": false,
    "grade": null,
    "completionDate": null
  }
]
```

---

### Add Course to My Record
```http
POST /api/course/my-courses
Authorization: Bearer {token}
Content-Type: application/json

{
  "courseRequirementId": 1,
  "isCompleted": true,
  "grade": 85.5,
  "completionDate": "2023-06-15T00:00:00Z"
}
```

**Response:**
```json
{
  "message": "Course added successfully",
  "id": 1
}
```

**Note:** 
- Automatically updates student profile's completed credits
- Cannot add the same course twice

**Error Responses:**

404 Not Found:
```json
{
  "error": "Course requirement not found"
}
```

400 Bad Request:
```json
{
  "error": "Course already added"
}
```

---

### Update Course Completion
```http
PATCH /api/course/my-courses/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "isCompleted": true,
  "grade": 90.0,
  "completionDate": "2023-06-15T00:00:00Z"
}
```

**Response:**
```json
{
  "message": "Course updated successfully"
}
```

**Note:** Marking course as completed automatically updates student profile's credit count

---

## ⭐ Document Rating Endpoints (NEW)

### Create or Update Rating
```http
POST /api/ratings
Authorization: Bearer {token}
Content-Type: application/json

{
  "documentVersionId": 12,
  "score": 85,
  "comments": "Excellent work! Well-researched and clearly written. Minor improvements needed in the conclusion."
}
```

**Authorization:** Requires `Advisor` or `Admin` role

**Validation:**
- Score must be between 1-100
- Only assigned advisor or admin can rate
- Updates existing rating if already rated

**Response (Created):**
```json
{
  "message": "Rating created successfully",
  "ratingId": 1,
  "score": 85
}
```

**Response (Updated):**
```json
{
  "message": "Rating updated successfully",
  "ratingId": 1,
  "score": 90
}
```

**Error Responses:**

400 Bad Request:
```json
{
  "error": "Score must be between 1 and 100"
}
```

403 Forbidden:
```json
{
  "error": "You are not authorized to rate this document"
}
```

---

### Get Ratings for Document Version
```http
GET /api/ratings/version/{versionId}
Authorization: Bearer {token}
```

**Response (Has Ratings):**
```json
{
  "hasRating": true,
  "averageScore": 87.5,
  "ratingCount": 2,
  "ratings": [
    {
      "id": 1,
      "documentVersionId": 12,
    "advisorUserId": "advisor-id-456",
      "score": 85,
    "comments": "Good work overall",
      "createdAt": "2024-01-15T10:00:00Z"
    },
  {
      "id": 2,
      "documentVersionId": 12,
      "advisorUserId": "advisor-id-789",
"score": 90,
      "comments": "Excellent research",
      "createdAt": "2024-01-16T14:30:00Z"
    }
  ]
}
```

**Response (No Ratings):**
```json
{
  "hasRating": false,
  "averageScore": null,
  "ratings": []
}
```

---

### Get Ratings by Advisor
```http
GET /api/ratings/by-advisor/{advisorId}
Authorization: Bearer {token}
```

**Authorization:** Admin or the advisor themselves

**Response:**
```json
{
  "totalRatings": 15,
  "averageScore": 82.5,
  "ratings": [
    {
      "id": 1,
  "documentVersionId": 12,
   "documentTitle": "Thesis Draft",
      "versionNo": 3,
      "score": 85,
      "comments": "Well done",
      "createdAt": "2024-01-15T10:00:00Z"
    }
  ]
}
```

---

### Get Ratings for My Documents (Student)
```http
GET /api/ratings/my-documents
Authorization: Bearer {token}
```

**Purpose:** Student can see all ratings received on their documents

**Response:**
```json
[
  {
    "id": 12,
    "documentId": 5,
    "documentTitle": "My Thesis",
    "versionNo": 3,
    "ratings": [
      {
        "id": 1,
        "advisorUserId": "advisor-id-456",
        "score": 85,
    "comments": "Good progress",
   "createdAt": "2024-01-15T10:00:00Z"
      }
    ]
  }
]
```

**Note:** Only includes document versions that have ratings

---

### Delete Rating
```http
DELETE /api/ratings/{id}
Authorization: Bearer {token}
```

**Authorization:** Admin or rating author

**Response:**
```json
{
  "message": "Rating deleted successfully"
}
```

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

### Test Claims (Debug)
```http
GET /api/notifications/test-claims
Authorization: Bearer {token}
```

**Purpose:** Debug JWT token claims to troubleshoot authentication issues.

**Response:**
```json
{
  "userId": "user-id-123",
  "isAuthenticated": true,
  "authenticationType": "Bearer",
  "name": "admin@local",
  "claims": [
 {
      "type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
      "value": "user-id-123"
    },
 {
      "type": "sub",
      "value": "user-id-123"
 },
    {
      "type": "email",
  "value": "admin@local"
    },
    {
  "type": "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
"value": "Admin"
    }
  ]
}
```

**Note:** This endpoint is available to all authenticated users in Development mode, and Admin only in Production.

**Usage:**
Use this endpoint to verify that your JWT token contains the correct claims. If you're experiencing authentication issues, check that your token includes at least one of these user ID claims:
- `sub`
- `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier`
- `nameidentifier`
- `uid`

---

## 📅 Submission Endpoints

### Get My Submissions
```http
GET /api/submissions/my
Authorization: Bearer {token}
```

**Authorization:**
- **Students:** See only their own submissions
- **Admin/Advisor:** See all submissions

**Response (Student):**
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

**Response (Admin/Advisor):**
```json
[
  {
    "id": 1,
    "studentId": "student-id-123",
    "dueDate": "2024-02-01T23:59:59Z",
    "status": "Completed"
  },
  {
    "id": 2,
  "studentId": "student-id-456",
    "dueDate": "2024-02-15T23:59:59Z",
    "status": "Pending"
  }
]
```

**Note:** 
- Changed in version 1.0.1
- Previously restricted to Student role only
- Now uses role-based filtering instead

---

### Create Submission
```http
POST /api/submissions
Authorization: Bearer {token}
Content-Type: application/json

{
  "studentId": "student-id-789",
  "documentId": 5,
  "dueDate": "2024-02-01T23:59:59Z",
  "notes": "Please complete final revisions"
}
```

**Parameters:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `studentId` | string | ✅ | Student user ID |
| `documentId` | integer | ❌ | Related document ID (optional) |
| `dueDate` | datetime | ✅ | Deadline (ISO 8601) |
| `notes` | string | ❌ | Additional notes |

**Response:**
```json
{
  "id": 4,
  "message": "Submission deadline created successfully"
}
```

**Authorization:**
- **Advisor**: Can only create submissions for their own students
- **Admin**: Can create submissions for any student

**Side Effects:**
- ✅ Student receives immediate notification
- ✅ Automatic deadline reminders start (3 days before)

**Note:** Requires `Advisor` or `Admin` role

**403 Forbidden (Advisor trying to assign to other students):**
```json
{
  "error": "You can only create submissions for your own students"
}
```

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

**Authorization:** Any authenticated user (returns current user's statistics)

**Response:**
```json
{
  "totalDocuments": 5,
  "totalVersions": 12,
  "pendingSubmissions": 2,
  "completedSubmissions": 3
}
```

**Note:** Returns statistics for the currently authenticated user, regardless of role.

---

### Advisor Summary
```http
GET /api/statistics/advisor/summary
Authorization: Bearer {token}
```

**Authorization:** Requires `Advisor` or `Admin` role

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

**Authorization:** Requires `Admin` role

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

## 👩‍🎓 Student Endpoints (Admin/Advisor)

### Get All Students
```http
GET /api/students?search=john&page=1&pageSize=20
Authorization: Bearer {token}
```

**Query Parameters:**
- `search` (optional): Search by email or username
- `page` (optional, default: 1): Page number
- `pageSize` (optional, default: 20): Items per page

**Response:**
```json
{
  "totalCount": 45,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3,
  "students": [
    {
      "id": "student-id-789",
      "userName": "john.doe@university.edu",
      "email": "john.doe@university.edu",
      "emailConfirmed": true,
      "documentCount": 5,
      "pendingSubmissions": 2,
      "hasAdvisor": true
    }
  ]
}
```

**Note:** Requires `Admin` or `Advisor` role

---

### Get Student Details
```http
GET /api/students/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "id": "student-id-789",
  "fullName": "Arda Yıldız",
  "email": "arda.yildiz@university.edu",
  "registrationNo": "20240001",
  "department": "Computer Science",
  "createdAt": "2024-01-10T09:00:00Z",
  "advisorId": "user-id-456",
  "status": "Active",
  "comments": [
    {
      "id": 8,
      "documentVersionId": 12,
 "authorUserId": "user-id-456",
      "content": "Please add more references",
      "createdAt": "2024-01-16T09:15:00Z"
    }
  ]
}
```

**Note:** Requires `Admin` or `Advisor` role

---

### Send Notification to Student
```http
POST /api/students/{id}/send-notification
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Document Review",
  "message": "Your document has been reviewed. Please log in to see the comments."
}
```

**Response:**
```json
{
  "message": "Notification sent to student"
}
```

**Note:** Requires `Admin` or `Advisor` role

**Error Responses:**

404 Not Found:
```json
{
  "error": "Student not found"
}
```

400 Bad Request:
```json
{
  "error": "User is not a student"
}
```

500 Internal Server Error:
```json
{
  "error": "Failed to send notification",
  "details": "Title cannot be null or empty",
  "innerError": "Parameter name: title"
}
```

---

### Send Bulk Notification

````````markdown
POST /api/students/send-notification-to-all
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Attention Required",
  "message": "Please check your dashboard for important updates."
}
```

**Response:**
```json
{
  "message": "Notification sent to all students"
}
```

**Note:** Requires `Admin` role

---

### Get Students Without Advisor
```http
GET /api/students/without-advisor
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": "student-id-790",
    "fullName": "Mehmet Öztürk",
    "email": "mehmet.ozturk@university.edu",
    "registrationNo": "20240002",
    "department": "Information Technology",
    "createdAt": "2024-01-10T09:00:00Z",
    "status": "Active"
  }
]
```

**Note:** Requires `Admin` or `Advisor` role

---

### Get Students With Pending Submissions
```http
GET /api/students/with-pending-submissions
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": "student-id-789",
    "fullName": "Arda Yıldız",
    "email": "arda.yildiz@university.edu",
    "registrationNo": "20240001",
    "department": "Computer Science",
    "createdAt": "2024-01-10T09:00:00Z",
    "advisorId": "user-id-456",
    "status": "Active",
    "pendingSubmissions": 2
  }
]
```

**Note:** Requires `Admin` or `Advisor` role

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

### Get Users Without Roles (NEW)
```http
GET /api/debug/users-without-roles
```

**Purpose:** Find users that were registered but don't have any role assigned

**Response:**
```json
{
  "count": 3,
  "users": [
    {
      "id": "user-id-123",
      "userName": "student1@example.com",
      "email": "student1@example.com",
      "emailConfirmed": false
    },
    {
"id": "user-id-456",
      "userName": "student2@example.com",
      "email": "student2@example.com",
      "emailConfirmed": false
    }
  ]
}
```

**Use Case:** 
- Identify users registered before the automatic role assignment was implemented
- Troubleshoot "user not appearing in student list" issues

---

### Fix Missing Roles (NEW)
```http
POST /api/debug/fix-missing-roles
```

**Purpose:** Automatically assign **Student** role to all users without any role

**Response:**
```json
{
  "message": "Missing roles fixed",
  "fixedCount": 5,
  "alreadyHadRole": 10,
  "totalUsers": 15,
  "errors": []
}
```

**What it does:**
1. Finds all users in the system
2. Checks each user's roles
3. If user has no roles, assigns "Student" role
4. Returns statistics of fixed users

**Use Case:**
- Fix legacy users registered before automatic role assignment
- Bulk fix after database issues
- Resolve "students not visible to admin" problems

**Error Response:**
```json
{
  "message": "Missing roles fixed",
  "fixedCount": 4,
  "alreadyHadRole": 10,
  "totalUsers": 15,
  "errors": [
    "user@example.com: Role 'Student' does not exist"
  ]
}
```

**⚠️ Important Notes:**
- This endpoint is for **development/troubleshooting only**
- Should be removed or restricted in production
- Only assigns "Student" role (not Advisor or Admin)
- Safe to run multiple times (idempotent)

---

## 📝 Common Response Codes

| Code | Description |
|------|-------------|
| 200 | Success |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Missing or invalid token |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found |
| 413 | Payload Too Large - File exceeds 10MB |
| 500 | Internal Server Error |

---

## 📁 File Upload Rules & Restrictions

### File Size Limit

**Maximum Size:** 10MB (10,485,760 bytes)

**Validation:** Automatic via middleware before reaching controller

**Error Response (413):**
```json
{
  "error": "File size exceeds limit",
  "message": "File 'document.pdf' is 15.50MB. Maximum allowed size is 10MB.",
  "maxSizeMB": 10,
  "fileSizeMB": 15.5
}
```

---

### Allowed File Types

| Type | Extension | MIME Type |
|------|-----------|-----------|
| **PDF** | `.pdf` | `application/pdf` |
| **Word Document** | `.docx` | `application/vnd.openxmlformats-officedocument.wordprocessingml.document` |
| **PowerPoint** | `.pptx` | `application/vnd.openxmlformats-officedocument.presentationml.presentation` |

**Validation:** Both extension and content-type are checked

**Error Response (400 - Invalid Extension):**
```json
{
  "error": "Invalid file type",
  "message": "File type '.xlsx' is not allowed. Only PDF, DOCX, and PPTX files are accepted.",
  "allowedTypes": [".pdf", ".docx", ".pptx"],
  "providedType": ".xlsx"
}
```

**Error Response (400 - Invalid Content Type):**
```json
{
  "error": "Invalid content type",
  "message": "Content type 'application/vnd.ms-excel' is not allowed.",
  "allowedTypes": ["PDF", "DOCX", "PPTX"]
}
```

---

### Frontend Validation Example

```javascript
const validateFile = (file) => {
  const maxSize = 10 * 1024 * 1024; // 10MB
  const allowedTypes = ['.pdf', '.docx', '.pptx'];
  const allowedMimeTypes = [
    'application/pdf',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    'application/vnd.openxmlformats-officedocument.presentationml.presentation'
  ];

  // Size check
  if (file.size > maxSize) {
    const sizeMB = (file.size / 1024 / 1024).toFixed(2);
    return {
  valid: false,
      error: `File is too large (${sizeMB}MB). Maximum size is 10MB.`
    };
  }

  // Extension check
  const extension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
  if (!allowedTypes.includes(extension)) {
    return {
      valid: false,
      error: `File type ${extension} is not allowed. Only PDF, DOCX, and PPTX files are accepted.`
    };
  }

  // MIME type check
  if (!allowedMimeTypes.includes(file.type)) {
    return {
      valid: false,
      error: `Invalid file format. Only PDF, DOCX, and PPTX files are accepted.`
    };
  }

  return { valid: true };
};

// Usage
const handleFileSelect = (event) => {
  const file = event.target.files[0];
  if (!file) return;

  const validation = validateFile(file);
  if (!validation.valid) {
    alert(validation.error);
    event.target.value = ''; // Clear selection
    return;
  }

  // Proceed with upload
  uploadFile(file);
};
```

---

## 🔔 Automatic Notifications

### Deadline Reminder System

**Background Service:** Runs every hour to check upcoming deadlines

**Trigger Conditions:**
- Submission deadline is within **3 days**
- Status is **"Pending"**
- No notification sent in the last 3 days

**Notification Content:**

**3 days before:**
```json
{
  "title": "Teslim Tarihi Yaklaşıyor",
  "message": "Teslim tarihinize 3 gün kaldı. Tarih: 15/02/2024 23:59",
  "type": 0,
  "relatedEntityId": "10",
  "relatedEntityType": "Submission"
}
```

**Same day (< 24 hours):**
```json
{
  "title": "Teslim Tarihi Yaklaşıyor",
  "message": "Teslim tarihinize 18 saat kaldı. Tarih: 15/02/2024 23:59",
  "type": 0,
  "relatedEntityId": "10",
  "relatedEntityType": "Submission"
}
```

**Notification Delivery:**
- ✅ Sent via `/api/notifications` endpoint
- ✅ Visible in student's notification list
- ✅ Includes deadline date and time
- ✅ One-time per deadline

---

## 🔐 Authorization Matrix

### Document Access Control

| Role | Can View | Conditions |
|------|----------|------------|
| **Student** | ✅ Own documents | `ownerUserId` matches their ID |
| **Advisor** | ✅ Assigned students' documents | `advisorUserId` matches their ID |
| **Admin** | ✅ All documents | No restrictions |

### Submission Creation

| Role | Can Create For | Restrictions |
|------|----------------|--------------|
| **Advisor** | ✅ Own students only | Document's `advisorUserId` must match |
| **Admin** | ✅ Any student | No restrictions |
| **Student** | ❌ Cannot create | - |

### Version Visibility

| Rule | Limit |
|------|-------|
| **Version Count** | Last 2 versions only |
| **Who Can View** | Owner, Advisor, Admin |
| **Older Versions** | Kept in database but not displayed |