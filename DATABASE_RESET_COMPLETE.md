# ?? Database Reset - Complete (2025-01-06)

**Date:** 2025-01-06  
**Action:** Database dropped and recreated from scratch  
**Status:** ? SUCCESS

---

## ?? What Was Done

### 1. Database Dropped
```sh
dotnet ef database drop --force
? Successfully dropped database 'AdvisorySystemDB'
```

### 2. Database Recreated
```sh
dotnet ef database update
? All 6 migrations applied successfully
```

---

## ?? Applied Migrations (In Order)

| # | Migration | Description | Status |
|---|-----------|-------------|--------|
| 1 | `InitialCreate` | Base tables (Documents, Users, etc.) | ? |
| 2 | `AddStudentProfileAndRatingFeatures` | Student profiles + advisor ratings | ? |
| 3 | `UpdateSubmissionAndFileValidation` | Submission improvements + notes | ? |
| 4 | `AddStudentAdvisorRelationship` | v3.1 advisor system | ? |
| 5 | `AddComprehensiveCourseSystem` | 117 courses + categories | ? |
| 6 | `UpdateStudentCourseRelationship` | Student enrollment system | ? |

---

## ?? Database Tables

### Created Tables (Clean State)

| Table | Purpose | Initial State |
|-------|---------|---------------|
| **AspNetUsers** | Identity users | Empty (will be seeded) |
| **AspNetRoles** | Roles | Empty (will be seeded) |
| **Documents** | Document management | Empty |
| **DocumentVersions** | File versions | Empty |
| **Comments** | Document comments | Empty |
| **DocumentRatings** | Advisor ratings (1-100) | Empty |
| **Submissions** | Deadline tracking | Empty |
| **Notifications** | User notifications | Empty |
| **StudentProfiles** | Student academic info | Empty |
| **Courses** | Course catalog | Empty (will be seeded) |
| **CourseCategories** | Course categories | Empty (will be seeded) |
| **Prerequisites** | Course dependencies | Empty (will be seeded) |
| **StudentCourses** | Student enrollments | Empty |
| **CourseRequirements** | Legacy (not used) | Empty |

---

## ?? Auto-Seeding (On Next Startup)

### IdentitySeeder
**Will Create:**
- 3 Roles: `Student`, `Advisor`, `Admin`
- 1 Admin: `admin@local` / `Admin123!`
- 3 Advisors: `advisor1-3@local` / `Advisor123!`
- 3 Students: `student1-3@local` / `Student123!`

### CourseSeeder
**Will Create:**
- 13 Course Categories
- 117 Courses (all curriculum)
- 13 Prerequisite relationships

---

## ?? Next Steps

### 1. Start Application
```sh
dotnet run
```

### 2. Verify Logs
Look for:
```
? Seeding identity data...
? Seeding courses...
? Done
```

### 3. Test Login
```http
POST https://localhost:7175/api/auth/login
Content-Type: application/json

{
  "email": "admin@local",
"password": "Admin123!"
}
```

### 4. Verify Courses
```http
GET https://localhost:7175/api/courses
Authorization: Bearer {token}
```

Expected: `{ "totalCount": 117, "courses": [...] }`

---

## ?? Default Test Users

### Admin Account
```
Email: admin@local
Password: Admin123!
Role: Admin
Permissions: Full access
```

### Advisor Accounts
```
advisor1@local / Advisor123!
advisor2@local / Advisor123!
advisor3@local / Advisor123!

Role: Advisor
Permissions: Manage own students
```

### Student Accounts
```
student1@local / Student123!
student2@local / Student123!
student3@local / Student123!

Role: Student
Permissions: View own data
AdvisorId: null (initially unassigned)
```

---

## ? Quick Verification

### After Starting App

1. **Check Users:**
```sql
SELECT COUNT(*) FROM AspNetUsers; -- Expected: 7
SELECT COUNT(*) FROM AspNetRoles; -- Expected: 3
```

2. **Check Courses:**
```sql
SELECT COUNT(*) FROM Courses; -- Expected: 117
SELECT COUNT(*) FROM CourseCategories; -- Expected: 13
SELECT COUNT(*) FROM Prerequisites; -- Expected: 13
```

3. **API Test:**
```javascript
// Login
const res = await fetch('https://localhost:7175/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'admin@local',
    password: 'Admin123!'
  })
});

const { token } = await res.json();

// Get courses
const courses = await fetch('https://localhost:7175/api/courses', {
  headers: { 'Authorization': `Bearer ${token}` }
});

const data = await courses.json();
console.log(`Total courses: ${data.totalCount}`); // Should be 117
```

---

## ?? Common Setup Tasks

### 1. Assign Advisor to Student
```http
POST /api/advisors/assign-to-student
Authorization: Bearer {admin-token}

{
  "studentEmail": "student1@local",
  "advisorEmail": "advisor1@local"
}
```

### 2. Student Enrolls in Semester 1
```javascript
// Login as student1
const token = await login('student1@local', 'Student123!');

// Get semester 1 courses
const sem1 = await api.get('/courses/by-semester/1');

// Enroll in all semester 1 courses
for (const course of sem1.data.courses) {
  await api.post('/student-courses/enroll', {
    courseId: course.id,
    semester: 1
  });
}
```

### 3. Create First Document
```javascript
// Login as student
const doc = await api.post('/documents', {
  title: 'Thesis Proposal',
  tags: 'thesis,research'
});

// Upload version
const formData = new FormData();
formData.append('file', pdfFile);
formData.append('notes', 'Initial draft');

await api.post(`/documents/${doc.data.id}/versions`, formData);
```

---

## ?? Database State Summary

### Before Reset
- ? Potentially inconsistent data
- ? Test data mixed with old migrations
- ? Unknown state

### After Reset
- ? Clean database
- ? All latest migrations
- ? Consistent structure
- ? Ready for seeding
- ? Production-ready schema

---

## ?? Troubleshooting

### If seeders don't run:
Check `Program.cs`:
```csharp
try
{
  await IdentitySeeder.SeedAsync(app.Services);
    await CourseSeeder.SeedCoursesAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error while seeding data");
}
```

### If courses are missing:
```csharp
// CourseSeeder checks if courses exist
if (await db.Courses.AnyAsync())
{
    return; // Courses already seeded
}
```

To force re-seed, delete courses first:
```sql
DELETE FROM StudentCourses;
DELETE FROM Prerequisites;
DELETE FROM Courses;
DELETE FROM CourseCategories;
```

Then restart application.

---

## ?? Summary

**Database:** ? Dropped and recreated  
**Migrations:** ? All 6 applied  
**Tables:** ? 14+ tables created  
**Seeders:** ? Will run on startup  
**State:** ? Fresh and clean  
**Next Step:** ?? `dotnet run`

---

## ?? Important Notes

1. **Seeders are idempotent** - They check if data exists before inserting
2. **Default passwords** - All test users use pattern `{Role}123!`
3. **No advisor assignments** - Students start without advisors
4. **No enrollments** - Students start with empty programs
5. **Clean slate** - Perfect for testing from scratch

---

**?? Your database is now completely fresh and ready!** ??

**Run:** `dotnet run` to start and seed data automatically.

