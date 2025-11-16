# Student Management API - Admin/Advisor Guide

## ?? Students Endpoints

All endpoints require `Admin` or `Advisor` role unless specified otherwise.

---

### 1. Get All Students (with Search)
```http
GET /api/students?search=john&page=1&pageSize=20
Authorization: Bearer {token}
```

**Query Parameters:**
- `search` (optional): Search by email or username
- `page` (optional, default: 1)
- `pageSize` (optional, default: 20)

**Response:**
```json
{
  "totalCount": 45,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3,
  "students": [
    {
  "id": "student-id-123",
      "userName": "john.doe@university.edu",
      "email": "john.doe@university.edu",
      "emailConfirmed": true,
      "documentCount": 5,
      "pendingSubmissions": 2,
      "hasAdvisor": true,
      "joinedAt": null
    }
  ]
}
```

---

### 2. Get Student Details
```http
GET /api/students/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "id": "student-id-123",
  "userName": "john.doe@university.edu",
  "email": "john.doe@university.edu",
  "emailConfirmed": true,
  "documents": [
    {
      "id": 5,
      "title": "Thesis Draft",
      "tags": "research,thesis",
      "createdAt": "2024-01-15T10:30:00Z",
      "versionCount": 3,
      "advisorId": "advisor-id-456"
    }
  ],
"submissions": [
    {
      "id": 3,
      "studentId": "student-id-123",
      "dueDate": "2024-02-01T23:59:59Z",
      "status": "Pending"
    }
  ],
  "unreadNotifications": 5
}
```

---

### 3. Send Notification to Single Student
```http
POST /api/students/{id}/send-notification
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Important Announcement",
  "message": "Please submit your documents by Friday",
  "type": 5,
  "relatedEntityId": null,
  "relatedEntityType": null
}
```

**Notification Types:**
| Value | Type |
|-------|------|
| 0 | DeadlineApproaching |
| 1 | NewComment |
| 2 | AdvisorAssigned |
| 3 | DocumentUploaded |
| 4 | SubmissionStatusChanged |
| 5 | General |

**Response:**
```json
{
  "message": "Notification sent to john.doe@university.edu"
}
```

---

### 4. Send Notification to Multiple Students
```http
POST /api/students/send-bulk-notification
Authorization: Bearer {token}
Content-Type: application/json

{
  "studentIds": ["student-id-123", "student-id-456", "student-id-789"],
  "title": "Group Meeting",
  "message": "Group meeting scheduled for Monday at 10 AM",
  "type": 5,
  "relatedEntityId": null,
  "relatedEntityType": null
}
```

**Response:**
```json
{
  "message": "Notification sent to 3 students",
  "successCount": 3,
  "failedCount": 0,
  "errors": []
}
```

---

### 5. Send Notification to All Students
```http
POST /api/students/send-notification-to-all
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "System Maintenance",
  "message": "The system will be down for maintenance this weekend",
  "type": 5
}
```

**Response:**
```json
{
  "message": "Notification sent to 42 students",
"totalStudents": 45,
  "successCount": 42
}
```

---

### 6. Get Students Without Advisor
```http
GET /api/students/without-advisor
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": "student-id-123",
    "userName": "john.doe@university.edu",
    "email": "john.doe@university.edu",
    "documentCount": 2
  }
]
```

---

### 7. Get Students With Pending Submissions
```http
GET /api/students/with-pending-submissions
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": "student-id-123",
    "userName": "john.doe@university.edu",
    "email": "john.doe@university.edu",
    "pendingSubmissions": 3,
    "nextDeadline": "2024-02-01T23:59:59Z"
  }
]
```

---

## ?? Frontend Examples

### Search Students
```javascript
const searchStudents = async (searchTerm) => {
  const token = localStorage.getItem('token');
  const response = await fetch(
    `https://localhost:7175/api/students?search=${encodeURIComponent(searchTerm)}&page=1&pageSize=20`,
    {
      headers: { 'Authorization': `Bearer ${token}` }
  }
  );
  return await response.json();
};

// Usage
const results = await searchStudents('john');
console.log(`Found ${results.totalCount} students`);
```

---

### Send Notification to Student
```javascript
const sendNotification = async (studentId, title, message) => {
  const token = localStorage.getItem('token');
  const response = await fetch(
    `https://localhost:7175/api/students/${studentId}/send-notification`,
    {
      method: 'POST',
headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        title: title,
      message: message,
     type: 5  // General notification
      })
    }
  );
  return await response.json();
};

// Usage
await sendNotification(
  'student-id-123',
  'Document Review Complete',
  'Your thesis has been reviewed. Please check the comments.'
);
```

---

### Send Bulk Notification
```javascript
const sendBulkNotification = async (studentIds, title, message) => {
  const token = localStorage.getItem('token');
  const response = await fetch(
    'https://localhost:7175/api/students/send-bulk-notification',
    {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
      studentIds: studentIds,
        title: title,
        message: message,
        type: 5
 })
    }
  );
  return await response.json();
};

// Usage
const selectedStudents = ['student-id-123', 'student-id-456'];
await sendBulkNotification(
  selectedStudents,
  'Meeting Reminder',
'Don\'t forget our meeting tomorrow at 10 AM'
);
```

---

### Get Students Without Advisor
```javascript
const getStudentsWithoutAdvisor = async () => {
  const token = localStorage.getItem('token');
  const response = await fetch(
    'https://localhost:7175/api/students/without-advisor',
    {
      headers: { 'Authorization': `Bearer ${token}` }
    }
  );
  return await response.json();
};

// React Component Example
const StudentsWithoutAdvisor = () => {
  const [students, setStudents] = useState([]);
  
  useEffect(() => {
    const fetchStudents = async () => {
      const data = await getStudentsWithoutAdvisor();
    setStudents(data);
    };
    fetchStudents();
  }, []);
  
  return (
    <div>
    <h2>Students Without Advisor ({students.length})</h2>
      <ul>
  {students.map(student => (
      <li key={student.id}>
  {student.userName} - {student.documentCount} documents
          </li>
 ))}
      </ul>
    </div>
  );
};
```

---

## ?? UI Component Examples

### Student Search Component (React)
```jsx
import { useState, useEffect } from 'react';

const StudentSearch = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [students, setStudents] = useState([]);
  const [loading, setLoading] = useState(false);
  const [selectedStudents, setSelectedStudents] = useState([]);
  
  const handleSearch = async () => {
    setLoading(true);
    try {
      const results = await searchStudents(searchTerm);
      setStudents(results.students);
    } catch (error) {
   console.error('Search failed:', error);
    } finally {
      setLoading(false);
    }
  };
  
  const toggleStudent = (studentId) => {
    setSelectedStudents(prev => 
      prev.includes(studentId)
        ? prev.filter(id => id !== studentId)
      : [...prev, studentId]
    );
  };
  
  const handleSendBulkNotification = async () => {
    const title = prompt('Notification title:');
    const message = prompt('Notification message:');

    if (title && message) {
      const result = await sendBulkNotification(selectedStudents, title, message);
      alert(result.message);
  setSelectedStudents([]);
    }
  };
  
  return (
    <div className="student-search">
      <div className="search-bar">
 <input
          type="text"
     placeholder="Search students by email or name..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
 onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
        />
   <button onClick={handleSearch} disabled={loading}>
          {loading ? 'Searching...' : 'Search'}
        </button>
      </div>
      
{selectedStudents.length > 0 && (
    <div className="bulk-actions">
          <span>{selectedStudents.length} students selected</span>
  <button onClick={handleSendBulkNotification}>
    Send Notification to Selected
     </button>
        </div>
      )}
      
      <div className="students-list">
        {students.map(student => (
  <div key={student.id} className="student-card">
<input
    type="checkbox"
     checked={selectedStudents.includes(student.id)}
       onChange={() => toggleStudent(student.id)}
            />
    <div className="student-info">
   <h3>{student.userName}</h3>
     <p>{student.email}</p>
      <div className="student-stats">
       <span>Documents: {student.documentCount}</span>
        <span>Pending: {student.pendingSubmissions}</span>
        <span>Advisor: {student.hasAdvisor ? 'Yes' : 'No'}</span>
  </div>
       </div>
      </div>
  ))}
   </div>
    </div>
  );
};
```

---

## ?? Use Cases

### 1. Find Students Needing Attention
```javascript
// Get students with pending submissions and no recent activity
const findStudentsNeedingAttention = async () => {
  const withPending = await fetch(
    'https://localhost:7175/api/students/with-pending-submissions',
    { headers: { 'Authorization': `Bearer ${token}` } }
  ).then(r => r.json());
  
  const withoutAdvisor = await fetch(
    'https://localhost:7175/api/students/without-advisor',
    { headers: { 'Authorization': `Bearer ${token}` } }
  ).then(r => r.json());
  
  return {
    pendingDeadlines: withPending,
    needsAdvisor: withoutAdvisor
  };
};
```

---

### 2. Send Deadline Reminders
```javascript
// Send reminders to students with pending submissions
const sendDeadlineReminders = async () => {
  const students = await fetch(
    'https://localhost:7175/api/students/with-pending-submissions',
    { headers: { 'Authorization': `Bearer ${token}` } }
  ).then(r => r.json());
  
  const studentIds = students.map(s => s.id);
  
  await sendBulkNotification(
    studentIds,
    'Deadline Reminder',
    'You have pending submissions due soon. Please check your dashboard.'
  );
};
```

---

### 3. Broadcast System Announcement
```javascript
// Send announcement to all students
const broadcastAnnouncement = async (title, message) => {
  const token = localStorage.getItem('token');
  const response = await fetch(
    'https://localhost:7175/api/students/send-notification-to-all',
    {
      method: 'POST',
   headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
   body: JSON.stringify({
        title: title,
        message: message,
        type: 5  // General
      })
    }
  );
  return await response.json();
};

// Usage
await broadcastAnnouncement(
  'System Maintenance',
  'The system will be unavailable this Saturday 2-4 PM for scheduled maintenance.'
);
```

---

## ? Authorization

All endpoints require:
- Valid JWT token
- `Admin` OR `Advisor` role
- Proper permissions

**Error Responses:**
- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - Insufficient permissions (not Admin/Advisor)
- `404 Not Found` - Student not found
- `400 Bad Request` - Invalid request data
