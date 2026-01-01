# ? Rating System - Fixed & Enhanced (v3.1.1)

**Date:** 2025-01-06  
**Feature:** Advisor can rate student documents  
**Status:** ? FIXED & WORKING

---

## ?? Feature Overview

### What Is It?
Advisors can rate their students' document versions with:
- **Score:** 1-100 range
- **Comments:** Optional feedback text
- **Version-specific:** Each version can have separate ratings

### Who Can Rate?
| Role | Permission | Condition |
|------|------------|-----------|
| **Admin** | ? All documents | No restrictions |
| **Advisor** | ? Own students' documents | `student.AdvisorId == advisorId` |
| **Student** | ? Cannot rate | - |

---

## ?? What Was Fixed

### Problem
Same as Comment 403 error - authorization was using wrong field:
- ? Old: `document.AdvisorUserId` (deprecated)
- ? New: `AppUser.AdvisorId` (v3.1 standard)

### Solution
Updated RatingsController to use v3.1 authorization model.

### Changes Made
1. ? Injected `UserManager<AppUser>`
2. ? Fetch document owner from database
3. ? Check `documentOwner.AdvisorId == advisorId` for authorization
4. ? Proper role-based access control

---

## ?? API Endpoints

### 1. Create or Update Rating
```http
POST /api/ratings
Authorization: Bearer {advisor-token}
Content-Type: application/json

{
  "documentVersionId": 12,
  "score": 85,
  "comments": "Excellent work! Well-researched and clearly written."
}
```

**Authorization:** Advisor or Admin only  
**Validation:**
- Score: 1-100 (required)
- Comments: Optional
- Advisor must be assigned to document owner

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
  "error": "Forbidden"
}
```

404 Not Found:
```json
{
  "error": "Document version not found"
}
```

---

### 2. Get Ratings for Document Version
```http
GET /api/ratings/version/{versionId}
Authorization: Bearer {token}
```

**Authorization:** Any authenticated user

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
  "ratingCount": 0,
  "ratings": []
}
```

---

### 3. Get Ratings by Advisor
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

### 4. Get Ratings for My Documents (Student)
```http
GET /api/ratings/my-documents
Authorization: Bearer {student-token}
```

**Authorization:** Student

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

---

### 5. Delete Rating
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

## ?? Frontend Examples

### React: Rate Document Version
```jsx
import { useState } from 'react';
import api from './api';

const RateDocument = ({ versionId }) => {
  const [score, setScore] = useState(50);
  const [comments, setComments] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      const response = await api.post('/ratings', {
        documentVersionId: versionId,
        score: parseInt(score),
        comments: comments.trim() || null
      });

      alert(`? ${response.data.message}`);
      setComments('');
    } catch (error) {
 if (error.response?.status === 403) {
 alert('? You can only rate your own students\' documents');
   } else if (error.response?.status === 400) {
        alert(`? ${error.response.data.error}`);
   } else {
alert('? Failed to save rating');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="rating-form">
      <h3>Rate This Document</h3>
      
      <div className="form-group">
        <label>Score (1-100):</label>
      <input
          type="range"
  min="1"
          max="100"
   value={score}
          onChange={(e) => setScore(e.target.value)}
     />
        <span className="score-display">{score}</span>
      </div>

      <div className="form-group">
        <label>Comments (optional):</label>
     <textarea
   rows="4"
 placeholder="Provide feedback..."
    value={comments}
          onChange={(e) => setComments(e.target.value)}
    />
      </div>

      <button type="submit" disabled={loading}>
        {loading ? 'Saving...' : 'Save Rating'}
      </button>
    </form>
  );
};

export default RateDocument;
```

---

### React: Display Ratings
```jsx
import { useEffect, useState } from 'react';
import api from './api';

const DocumentRatings = ({ versionId }) => {
  const [ratings, setRatings] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchRatings = async () => {
      try {
     const response = await api.get(`/ratings/version/${versionId}`);
  setRatings(response.data);
      } catch (error) {
        console.error('Failed to load ratings:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchRatings();
  }, [versionId]);

  if (loading) return <div>Loading ratings...</div>;

  if (!ratings?.hasRating) {
 return <div className="no-ratings">No ratings yet</div>;
  }

  return (
    <div className="ratings-section">
      <div className="average-score">
        <h4>Average Score</h4>
        <div className="score-badge">{ratings.averageScore.toFixed(1)}</div>
        <p>{ratings.ratingCount} rating(s)</p>
      </div>

      <div className="ratings-list">
  <h4>Individual Ratings</h4>
        {ratings.ratings.map(rating => (
          <div key={rating.id} className="rating-item">
            <div className="rating-header">
     <span className="score">{rating.score}/100</span>
           <span className="date">
     {new Date(rating.createdAt).toLocaleDateString()}
 </span>
          </div>
        {rating.comments && (
       <p className="comments">{rating.comments}</p>
      )}
          </div>
        ))}
      </div>
    </div>
  );
};

export default DocumentRatings;
```

---

### React: Student - View My Ratings
```jsx
import { useEffect, useState } from 'react';
import api from './api';

const MyRatings = () => {
  const [documents, setDocuments] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchRatings = async () => {
  try {
    const response = await api.get('/ratings/my-documents');
      setDocuments(response.data);
      } catch (error) {
        console.error('Failed to load ratings:', error);
   } finally {
        setLoading(false);
   }
    };

    fetchRatings();
  }, []);

  if (loading) return <div>Loading your ratings...</div>;

if (documents.length === 0) {
  return <div>You haven't received any ratings yet</div>;
  }

  return (
    <div className="my-ratings">
      <h2>My Document Ratings</h2>
 
      {documents.map(doc => (
    <div key={doc.id} className="document-ratings">
       <h3>{doc.documentTitle} - Version {doc.versionNo}</h3>
     
          {doc.ratings.map(rating => (
            <div key={rating.id} className="rating-card">
   <div className="score-badge">{rating.score}/100</div>
          <div className="rating-details">
  {rating.comments && <p>{rating.comments}</p>}
       <small>
         {new Date(rating.createdAt).toLocaleDateString()}
             </small>
              </div>
      </div>
          ))}
        </div>
      ))}
    </div>
  );
};

export default MyRatings;
```

---

## ?? Testing

### Test 1: Advisor Rates Own Student's Document ?
```bash
# 1. Login as advisor
POST /api/auth/login
{ "email": "advisor1@local", "password": "Advisor123!" }

# 2. Rate student's document
POST /api/ratings
{
  "documentVersionId": 12,
  "score": 85,
  "comments": "Excellent work!"
}

# Expected: 200 OK
```

### Test 2: Advisor Updates Rating ?
```bash
# Same advisor rates same version again
POST /api/ratings
{
  "documentVersionId": 12,
  "score": 90,
  "comments": "Even better after revisions!"
}

# Expected: 200 OK, "Rating updated successfully"
```

### Test 3: Advisor Cannot Rate Other Student's Document ?
```bash
# Advisor tries to rate document of student NOT assigned to them
POST /api/ratings
{
  "documentVersionId": 20,
  "score": 75
}

# Expected: 403 Forbidden
```

### Test 4: Student Views Own Ratings ?
```bash
# Login as student
POST /api/auth/login
{ "email": "student1@local", "password": "Student123!" }

# Get ratings
GET /api/ratings/my-documents

# Expected: 200 OK with ratings array
```

---

## ?? Authorization Logic (v3.1)

### Before (Broken):
```csharp
// ? Wrong field
var isAssignedAdvisor = version.Document.AdvisorUserId == userId;
```

### After (Fixed):
```csharp
// ? Correct field (v3.1)
var documentOwner = await _userManager.FindByIdAsync(version.Document.OwnerUserId);

bool canRate = false;

if (isAdmin)
    canRate = true;
else if (isAdvisor && documentOwner.AdvisorId == userId)
    canRate = true;

if (!canRate)
    return Forbid();
```

---

## ?? Summary

| Feature | Status |
|---------|--------|
| **Create Rating** | ? Working |
| **Update Rating** | ? Working |
| **View Ratings** | ? Working |
| **Delete Rating** | ? Working |
| **Authorization** | ? Fixed (v3.1) |
| **Student View** | ? Working |

| Aspect | Before | After |
|--------|--------|-------|
| **Advisor Rating** | ? 403 Error | ? Works |
| **Authorization** | ? Wrong field | ? v3.1 compliant |
| **Student Check** | ? document.AdvisorUserId | ? owner.AdvisorId |
| **UserManager** | ? Not injected | ? Injected |

---

## ? Checklist

- [x] Fix authorization logic
- [x] Add UserManager injection
- [x] Update to v3.1 model
- [x] Build successful
- [x] Test advisor rating
- [x] Test student view
- [x] Documentation complete

---

**Status:** ? FIXED & WORKING  
**Build:** ? Successful  
**Version:** 3.1.1  
**Ready for:** Production

