# ?? Migration Guide: v2.1 ? v3.0

**Date:** December 20, 2024  
**Breaking Changes:** Yes  
**Migration Time:** ~15 minutes

---

## ?? What Changed?

### v2.1 (Old - Complex)
- ? Multiple endpoints for different roles
- ? Student/Advisor could view/manage assignments
- ? Complex authorization logic
- ? Document-based advisor assignment (deprecated but still there)

### v3.0 (New - Simplified)
- ? **Admin-only** management
- ? 4 simple endpoints
- ? Unified assignment/update endpoint
- ? Complete student list with advisor info
- ? Ready-to-use admin UI

---

## ?? Breaking Changes

### 1. Controller Access Control

**Before (v2.1):**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Any authenticated user
public class AdvisorsController : ControllerBase
{
    [HttpPost("assign-to-student")]
    [Authorize(Roles = "Admin")] // Per-method authorization
    public async Task<IActionResult> AssignAdvisorToStudent(...)
    
    [HttpGet("my-advisor")] // Student can access
    public async Task<IActionResult> GetMyAdvisor(...)
    
    [HttpGet("my-students")]
    [Authorize(Roles = "Advisor")] // Advisor can access
 public async Task<IActionResult> GetMyStudents(...)
}
```

**After (v3.0):**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Controller-level: Admin ONLY
public class AdvisorsController : ControllerBase
{
    [HttpPost("assign")] // Simplified route
    public async Task<IActionResult> AssignAdvisorToStudent(...)
 
    [HttpDelete("remove/{studentId}")] // Simplified route
    public async Task<IActionResult> RemoveAdvisorFromStudent(...)
    
[HttpGet("{advisorId}")] // New endpoint
    public async Task<IActionResult> GetAdvisorDetails(...)
}
```

### 2. API Endpoints

| v2.1 Endpoint | v3.0 Endpoint | Change |
|---------------|---------------|--------|
| `POST /api/advisors/assign-to-student` | `POST /api/advisors/assign` | ? Simplified route |
| `GET /api/advisors/my-advisor` | ? REMOVED | Student endpoint removed |
| `GET /api/advisors/my-students` | ? REMOVED | Advisor endpoint removed |
| `DELETE /api/advisors/remove-from-student/{id}` | `DELETE /api/advisors/remove/{id}` | ? Simplified route |
| ? N/A | `GET /api/advisors/{advisorId}` | ? NEW endpoint |

### 3. Request/Response Format

**Assignment Request - SAME:**
```json
{
  "studentId": "user-id-123",
"advisorId": "user-id-456"
}
```

**Assignment Response - ENHANCED:**
```json
{
  "message": "Öðretmen baþarýyla atandý",
  "studentId": "...",
  "studentName": "...",
  "advisorId": "...",
  "advisorName": "...",
  "isUpdate": false  // NEW: indicates if this was an update
}
```

---

## ?? Backend Migration Steps

### Step 1: Update Controller (Already Done ?)

The `AdvisorsController.cs` has been completely refactored.

**No action needed** - already updated in your workspace.

### Step 2: Update Frontend API Calls

**Before (v2.1):**
```javascript
// Different endpoints for different purposes
const assignAdvisor = async (studentId, advisorId) => {
  await fetch('/api/advisors/assign-to-student', {
    method: 'POST',
    body: JSON.stringify({ studentId, advisorId })
  });
};

const getMyAdvisor = async () => {
  await fetch('/api/advisors/my-advisor'); // Student endpoint
};

const getMyStudents = async () => {
  await fetch('/api/advisors/my-students'); // Advisor endpoint
};

const removeAdvisor = async (studentId) => {
  await fetch(`/api/advisors/remove-from-student/${studentId}`, {
    method: 'DELETE'
  });
};
```

**After (v3.0):**
```javascript
// Simplified admin-only endpoints
const assignAdvisor = async (studentId, advisorId) => {
  await fetch('/api/advisors/assign', { // Simplified route
    method: 'POST',
    body: JSON.stringify({ studentId, advisorId })
  });
};

// Student/Advisor endpoints removed - use /api/students instead
const getAllStudents = async () => {
  const response = await fetch('/api/students?pageSize=1000');
  const data = await response.json();
  // Each student now has advisor info: hasAdvisor, advisor { id, userName, email }
};

const removeAdvisor = async (studentId) => {
  await fetch(`/api/advisors/remove/${studentId}`, { // Simplified route
    method: 'DELETE'
  });
};

// NEW: Get advisor details with students
const getAdvisorDetails = async (advisorId) => {
  const response = await fetch(`/api/advisors/${advisorId}`);
return await response.json();
};
```

### Step 3: Update Student Viewing Logic

**Before (v2.1):**
```javascript
// Student had to call separate endpoint
const student = await getStudent(studentId);
const advisorInfo = await getMyAdvisor(); // Separate call
```

**After (v3.0):**
```javascript
// Advisor info included in student response
const student = await getStudent(studentId);
if (student.hasAdvisor) {
  console.log('Advisor:', student.advisor.userName);
}
```

### Step 4: Remove Student/Advisor UI Components

**Remove these features:**
- ? Student "View My Advisor" page
- ? Advisor "View My Students" page
- ? Advisor "Request Assignment" functionality

**Replace with:**
- ? Admin-only advisor management panel
- ? Advisor info displayed in student profile (read-only)

---

## ?? Frontend Migration

### Old Student Profile (Remove)

```html
<!-- REMOVE THIS -->
<div class="advisor-section">
  <h3>Öðretmenim</h3>
  <button onclick="viewMyAdvisor()">Öðretmenimi Gör</button>
  <button onclick="requestAdvisor()">Öðretmen Talebi Gönder</button>
</div>
```

### New Student Profile (Add)

```html
<!-- ADD THIS (Read-only display) -->
<div class="advisor-section">
  <h3>Öðretmenim</h3>
  <div id="advisor-info">
    <!-- Populated from student.advisor -->
  </div>
</div>

<script>
async function loadStudentProfile() {
  const response = await fetch(`/api/students/${studentId}`);
  const student = await response.json();
  
  const advisorDiv = document.getElementById('advisor-info');
  if (student.hasAdvisor) {
    advisorDiv.innerHTML = `
      <p><strong>Ad:</strong> ${student.advisor.userName}</p>
      <p><strong>Email:</strong> ${student.advisor.email}</p>
    `;
  } else {
  advisorDiv.innerHTML = '<p><em>Henüz öðretmen atanmamýþ</em></p>';
  }
}
</script>
```

### Admin Panel (New)

Use the complete admin panel from [ADMIN_ADVISOR_MANAGEMENT_API.md](ADMIN_ADVISOR_MANAGEMENT_API.md)

---

## ?? Testing Checklist

### Backend Tests

- [ ] `GET /api/advisors` returns all advisors (Admin only)
- [ ] `POST /api/advisors/assign` assigns advisor to student
- [ ] `POST /api/advisors/assign` updates existing advisor
- [ ] `DELETE /api/advisors/remove/{id}` removes advisor
- [ ] `GET /api/advisors/{id}` returns advisor with students
- [ ] Non-admin users get 403 Forbidden

### Frontend Tests

- [ ] Admin can view all students with advisor info
- [ ] Admin can search students by email/name
- [ ] Admin can assign advisor via modal
- [ ] Admin can update existing advisor
- [ ] Admin can remove advisor
- [ ] Student can view their advisor (read-only)
- [ ] Advisor endpoints removed from student/advisor UI
- [ ] Notifications sent correctly (student, new advisor, old advisor)

---

## ?? Database Changes

**No database migration needed!**

The `AppUser.AdvisorId` field already exists from v2.1.

---

## ?? Deployment Steps

### 1. Update Code
```bash
git pull origin main
dotnet restore
dotnet build
```

### 2. No Migration Needed
```bash
# Skip - no database changes
```

### 3. Deploy
```bash
dotnet publish -c Release -o ./publish
# Deploy to your server
```

### 4. Test
```bash
curl -X GET https://your-server/api/advisors \
  -H "Authorization: Bearer ADMIN_TOKEN"
```

---

## ?? Rollback Plan

If you need to rollback to v2.1:

1. **Revert code:**
```bash
git revert <commit-hash>
```

2. **No database rollback needed** (same schema)

3. **Restore old frontend files**

---

## ? FAQ

### Q: Can students still view their advisor?
**A:** Yes, via `GET /api/students/{id}` - advisor info is included in response.

### Q: Can advisors still view their students?
**A:** No dedicated endpoint. Admins can use `GET /api/advisors/{advisorId}` to see assigned students.

### Q: What happens to existing assignments?
**A:** Nothing changes - all existing `AppUser.AdvisorId` values remain intact.

### Q: Do I need to run migrations?
**A:** No - database schema is unchanged from v2.1.

### Q: Will old API calls break?
**A:** Yes - routes changed:
- `/api/advisors/assign-to-student` ? `/api/advisors/assign`
- `/api/advisors/remove-from-student/{id}` ? `/api/advisors/remove/{id}`
- `/api/advisors/my-advisor` ? REMOVED
- `/api/advisors/my-students` ? REMOVED

---

## ?? Resources

- **New API Docs:** [ADMIN_ADVISOR_MANAGEMENT_API.md](ADMIN_ADVISOR_MANAGEMENT_API.md)
- **Admin UI Example:** See complete HTML in API docs
- **Updated README:** [README.md](README.md)

---

**Migration Status:** ? Complete  
**Estimated Time:** 15 minutes  
**Complexity:** Low (frontend changes only)
