# AdvisorySystem API — Technical Documentation

**Version:** 1.0  
**Framework:** .NET 8 (ASP.NET Core Web API)  
**Database:** Azure SQL Server  
**Deployment:** Azure App Service (`bedes`)  
**Live URL:** `https://bedes.azurewebsites.net`  
**Frontend:** `https://nice-sand-008811f03.7.azurestaticapps.net`

---

## Table of Contents

1. [Overview](#1-overview)
2. [Technology Stack](#2-technology-stack)
3. [Project Structure](#3-project-structure)
4. [Authentication & Authorization](#4-authentication--authorization)
5. [Database Schema](#5-database-schema)
6. [API Endpoints](#6-api-endpoints)
7. [Rate Limiting](#7-rate-limiting)
8. [File Storage](#8-file-storage)
9. [Notification System](#9-notification-system)
10. [Course Scheduling Engine](#10-course-scheduling-engine)
11. [Deployment](#11-deployment)
12. [Configuration](#12-configuration)

---

## 1. Overview

AdvisorySystem is a university advisory platform where students can manage course registrations, upload documents, and communicate with their academic advisors. The backend is a RESTful API built with ASP.NET Core.

**Core features:**
- Student course registration with conflict detection
- Document upload and versioning
- Advisor–student assignment and management
- Real-time notifications
- Automated schedule generation
- Role-based access control (Student / Advisor / Admin)

---

## 2. Technology Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 8 |
| Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core 8 |
| Database | SQL Server (Azure SQL) |
| Identity | ASP.NET Core Identity |
| Authentication | JWT Bearer Tokens |
| File Storage | Azure Blob Storage / Local fallback |
| Monitoring | Application Insights |
| API Docs | Swagger / OpenAPI (Swashbuckle) |
| CI/CD | GitHub Actions |
| Hosting | Azure App Service |

### NuGet Packages

```
Azure.Storage.Blobs         12.19.1
Microsoft.ApplicationInsights.AspNetCore 2.22.0
Microsoft.EntityFrameworkCore.SqlServer  8.0.0
Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.0
Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0
Swashbuckle.AspNetCore   6.6.2
Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore 8.0.0
```

---

## 3. Project Structure

```
AdvisorySystem.Api/
??? Controllers/        # HTTP endpoint handlers
?   ??? AuthController.cs
?   ??? CourseSelectionController.cs
?   ??? SectionEnrollmentController.cs
?   ??? ScheduleController.cs
?   ??? DocumentsController.cs
?   ??? CommentsController.cs
???? SubmissionsController.cs
?   ??? NotificationsController.cs
?   ??? StudentsController.cs
?   ??? AdvisorsController.cs
???? StudentProfileController.cs
?   ??? StatisticsController.cs
?   ??? ...
??? Data/
?   ??? AppDbContext.cs   # EF Core context + all entity classes
?   ??? IdentitySeeder.cs
?   ??? DesignTimeDbContextFactory.cs
??? Models/
?   ??? AppUser.cs        # Identity user model
??? Services/
?   ??? ICourseScheduler.cs / CourseScheduler.cs
?   ??? INotificationService.cs / NotificationService.cs
?   ??? DeadlineNotificationService.cs  # Background service
?   ??? IFileStorage.cs / AzureBlobStorage.cs / LocalFileStorage.cs
?   ??? CourseSeeder.cs
?   ??? CourseScheduleSeeder.cs
??? Middleware/
?   ??? FileSizeValidationMiddleware.cs
??? Migrations/ # EF Core migration history
??? Program.cs # App bootstrap & DI configuration
??? appsettings.json
??? appsettings.Production.json
```

---

## 4. Authentication & Authorization

### JWT Authentication

All protected endpoints require a `Bearer` token in the `Authorization` header.

**Token generation flow:**
1. `POST /api/auth/login` ? validates credentials via ASP.NET Identity
2. Server generates JWT with claims: `sub`, `email`, `name`, `jti`, `ClaimTypes.Role`
3. Token is valid for **1440 minutes (24 hours)** by default
4. `POST /api/auth/refresh` renews the token for an authenticated user

**Token claims:**
```
sub          ? User GUID (primary identifier)
ClaimTypes.NameIdentifier ? User GUID (ASP.NET compatibility)
ClaimTypes.Role        ? Student | Advisor | Admin
email       ? User email
jti        ? Unique token ID
```

### Roles

| Role | Description |
|---|---|
| `Student` | Default role on registration. Can enroll in courses, upload documents |
| `Advisor` | Assigned by Admin. Manages own students, creates submissions, rates documents |
| `Admin` | Full access. User management, schedule generation, bulk notifications |

> **Note:** Admin cannot act as a student (cannot view student profiles directly, cannot enroll in courses).

### Seeded Default Users

| Email | Password | Role |
|---|---|---|
| `admin@local` | `Admin123!` | Admin |
| `advisor1@local` | `Advisor123!` | Advisor |
| `advisor2@local` | `Advisor123!` | Advisor |
| `advisor3@local` | `Advisor123!` | Advisor |
| `student1@local` | `Student123!` | Student |
| `student2@local` | `Student123!` | Student |
| `student3@local` | `Student123!` | Student |

---

## 5. Database Schema

All entity classes are defined in `Data/AppDbContext.cs`.

### Core Entities

#### AppUser *(extends IdentityUser)*
| Field | Type | Notes |
|---|---|---|
| `Id` | `string` | GUID, PK |
| `UserName` | `string` | Email used as username |
| `Email` | `string` | |
| `AdvisorId` | `string?` | FK ? AppUser (self-reference) |

#### Course
| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | PK |
| `CourseCode` | `nvarchar(20)` | Unique index |
| `CourseName` | `nvarchar(300)` | |
| `Credits` / `ECTS` | `int` | |
| `TheoryHours` / `PracticeHours` | `int` | Weekly hours |
| `Semester` | `int?` | 1–8 |
| `IsElective` | `bool` | |
| `CategoryId` | `int` | FK ? CourseCategory |

#### CourseSchedule
| Field | Type | Notes |
|---|---|---|
| `CourseId` | `int` | FK ? Course |
| `SectionCode` | `string` | A / B / C |
| `DayOfWeek` | `DayOfWeek` | |
| `StartTime` / `EndTime` | `TimeSpan` | |
| `InstructorName` | `string?` | |
| `MaxCapacity` | `int` | Default: 50 |
| `Semester` | `int` | |

Index: `(CourseId, Semester, DayOfWeek, StartTime)`

#### StudentCourseSection
Student's active course enrollment.
| Field | Type | Notes |
|---|---|---|
| `StudentId` | `string` | FK ? AppUser |
| `CourseId` | `int` | FK ? Course |
| `SectionCode` | `string` | |
| `IsCompleted` | `bool` | |

#### Document / DocumentVersion
Documents support versioning. Only the last 2 versions are displayed in the UI.

#### Notification
| Field | Type | Notes |
|---|---|---|
| `UserId` | `string` | FK ? AppUser |
| `Title` | `string` | |
| `Message` | `string` | |
| `Type` | `NotificationType` | Enum 0–5 |
| `IsRead` | `bool` | |
| `RelatedEntityId` | `string?` | |
| `RelatedEntityType` | `string?` | "Document", "Submission" etc. |

**NotificationType enum:**
```
0 = DeadlineApproaching
1 = NewComment
2 = AdvisorAssigned
3 = DocumentUploaded
4 = SubmissionStatusChanged
5 = General
```

### Key Relationships

```
AppUser  ???????? (AdvisorId) ???????  AppUser   (Self-reference, 1:N)
AppUser  ????????????????????????????? StudentCourseSection (1:N)
AppUser  ????????????????????????????? StudentProfile       (1:1)
Course   ????????????????????????????? CourseSchedule       (1:N, CASCADE)
Course   ????????????????????????????? StudentCourseSection (1:N, CASCADE)
Document ????????????????????????????? DocumentVersion      (1:N, CASCADE)
```

### Migrations

EF Core code-first migrations. Applied automatically on startup via:

```csharp
await db.Database.MigrateAsync();
```

Notable migrations:
- `InitialCreate` — base schema
- `AddCourseSchedulingSystem` — courses, schedules, sections
- `RemoveSemesterFromStudentCourseSection` — schema cleanup
- `FixTurkishCollation` — no-op (Azure SQL availability group workaround)

---

## 6. API Endpoints

Base URL: `/api`

### Auth

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/auth/register` | Public | Register new user (Student role) |
| POST | `/auth/login` | Public | Login, returns JWT |
| POST | `/auth/refresh` | JWT | Renew token |
| GET | `/auth/validate` | JWT | Validate token + return roles |

### Course Selection

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/course-selection/available?studentId=` | JWT | List all courses with enrollment status |
| POST | `/course-selection/enroll` | JWT | Enroll in a course (atomic transaction) |
| DELETE | `/course-selection/unenroll/{courseId}?studentId=` | JWT | Unenroll from a course |

> Advisors can pass `?studentId=` / `dto.StudentId` to act on behalf of their students.

### Section Enrollment

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/section-enrollment/my-enrollments` | JWT | Get current enrollments with schedule |
| POST | `/section-enrollment/enroll` | JWT | Enroll with auto section selection |
| DELETE | `/section-enrollment/unenroll/{courseId}` | JWT | Unenroll |

### Schedule

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/schedule/available` | JWT | All courses grouped by section |
| GET | `/schedule/search?query=&semester=` | JWT | Search courses |
| GET | `/schedule/semester/{n}` | JWT | Schedule for a semester |
| GET | `/schedule/week/{n}` | JWT | Weekly grid view |
| POST | `/schedule/generate/{n}` | Admin | Auto-generate schedule |
| GET | `/schedule/conflicts/{n}` | Admin | List schedule conflicts |
| PUT | `/schedule/{id}` | Admin | Update a schedule entry |
| DELETE | `/schedule/semester/{n}` | Admin | Delete all schedules for semester |

### Documents

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/documents` | JWT | List documents (role-filtered) |
| POST | `/documents` | Student | Create new document |
| POST | `/documents/{id}/versions` | JWT | Upload new version (max 10MB) |
| GET | `/documents/{id}/versions` | JWT | List last 2 versions |
| GET | `/documents/download/{versionId}` | JWT | Download file |
| GET | `/documents/preview/{versionId}` | JWT | Inline PDF preview |
| GET | `/documents/metadata/{versionId}` | JWT | File metadata |

### Submissions

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/submissions/my` | JWT | List submissions (role-filtered) |
| POST | `/submissions` | Advisor | Create deadline for own student |

### Comments

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/comments/version/{id}` | JWT | List comments on a version |
| POST | `/comments` | JWT | Add comment (owner / advisor / admin) |
| DELETE | `/comments/{id}` | JWT | Delete own comment |

### Notifications

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/notifications?isRead=` | JWT | Get notifications |
| GET | `/notifications/unread-count` | JWT | Unread count |
| PATCH | `/notifications/{id}/read` | JWT | Mark as read |
| PATCH | `/notifications/mark-all-read` | JWT | Mark all read |
| POST | `/notifications` | Admin | Create notification (test) |

### Students

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/students` | Admin/Advisor | List students |
| GET | `/students/{id}` | Admin/Advisor | Student detail |
| GET | `/students/my-students` | Advisor | Own assigned students |
| POST | `/students/{id}/send-notification` | Admin/Advisor | Send notification |
| POST | `/students/send-bulk-notification` | Admin/Advisor | Bulk notification |

### Advisors

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/advisors` | Admin | List all advisors |
| GET | `/advisors/{id}` | Admin | Advisor detail + students |
| POST | `/advisors/assign` | Admin | Assign advisor to student |
| DELETE | `/advisors/remove/{studentId}` | Admin | Remove advisor |

### Student Profile

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/studentprofile/me` | Student | Own profile |
| POST | `/studentprofile` | Student | Create / update profile |
| GET | `/studentprofile/{studentId}` | Advisor | View student profile |
| GET | `/studentprofile/check-prerequisites` | Student | Check prerequisites |

### Statistics

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/statistics/student/summary` | JWT | Document/submission counts |
| GET | `/statistics/advisor/summary` | Advisor/Admin | Assignment stats |
| GET | `/statistics/admin/overview` | Admin | System-wide overview |

### Health

```
GET /health    ? Database connectivity check (no auth required)
```

---

## 7. Rate Limiting

Implemented via ASP.NET Core `RateLimiter` middleware. Policies are applied per-endpoint.

| Policy | Limit | Window | Used By |
|---|---|---|---|
| `auth-strict` | 5 req | 1 min | Login, Register |
| `auth-relaxed` | 30 req | 1 min | Refresh, Validate |
| `upload` | 10 req | 1 min | File upload |
| `download` | 50 req | 1 min | File download/preview |
| `search` | 30 req | 1 min | Course search |
| `standard` | 60 req | 1 min | Most endpoints |
| `admin` | 100 req | 1 min | Admin endpoints |
| Global | 100 req | 1 min | All (per user/IP) |

Partition key: authenticated user ID, falling back to IP address.  
Rejected requests return **HTTP 429** with a `retryAfter` value.

---

## 8. File Storage

Abstracted via `IFileStorage` interface with two implementations:

### AzureBlobStorage
Used in production when `Azure:StorageConnectionString` is set.  
Files are stored in the `documents` container.

### LocalFileStorage
Fallback for development. Files are saved to `wwwroot/uploads`.

**Constraints:**
- Max file size: **10 MB** (`10485760` bytes)
- Enforced by `FileSizeValidationMiddleware`
- Allowed MIME types: PDF, DOCX, PPTX (validated at controller level)

---

## 9. Notification System

### Real-time (on-demand)
Notifications are created synchronously when events occur:
- Advisor assigned/removed ? `AdvisorsController`
- New comment on document ? `CommentsController`
- New submission deadline ? `SubmissionsController`
- Course enrollment (if needed) ? `CourseSelectionController`

### Background Service (DeadlineNotificationService)
A `BackgroundService` that runs every **1 hour**.  
Checks for submissions with status `Pending` and due date within the next **3 days**.  
Sends `DeadlineApproaching` notifications, avoiding duplicates via a 3-day lookback window.

---

## 10. Course Scheduling Engine

`CourseScheduler` (`Services/CourseScheduler.cs`) handles automated schedule generation.

### Algorithm
1. Fetches non-elective courses for the target semester
2. Orders by total weekly hours (descending) to prioritize heavier courses
3. For each section (A, B, C):
   - Splits course hours into sessions (max 2h per session)
   - Assigns sessions to available time slots (Mon–Fri, 09:00–17:00)
   - Tracks used slots per day to avoid same-slot conflicts
4. Saves all generated `CourseSchedule` records

**Available time slots:** 09:00, 10:00, 11:00, 13:00, 14:00, 15:00, 16:00 (each 1 hour)

### Conflict Detection
`DetectConflictsAsync` scans all schedules for a given semester and records any time overlap in the `ScheduleConflicts` table.

### Student-level Conflict Check
Before enrolling a student, `CheckScheduleConflict` compares the new course's schedule against all of the student's current enrollments, checking same-day time overlaps.

---

## 11. Deployment

### CI/CD Pipeline
GitHub Actions workflow: `.github/workflows/master_bedes.yml`

**Trigger:** Push to `master` branch (or manual dispatch)

**Steps:**
1. `dotnet build --configuration Release`
2. `dotnet publish -c Release`
3. Azure OIDC login (Federated Identity)
4. Deploy to Azure Web App `bedes` (Production slot)

### Infrastructure
| Resource | Type | Name |
|---|---|---|
| API | Azure App Service | `bedes` |
| Database | Azure SQL Database | — |
| File Storage | Azure Blob Storage | `documents` container |
| Frontend | Azure Static Web Apps | `nice-sand-008811f03` |
| Monitoring | Application Insights | — |

### Startup Sequence
`Program.cs` runs the following on boot:
1. Apply EF Core migrations (`MigrateAsync`)
2. Seed identity roles and default users (`IdentitySeeder`)
3. Seed course catalog (`CourseSeeder`) — clears and re-seeds if courses exist
4. Seed course schedules (`CourseScheduleSeeder`) — skips if schedules exist

---

## 12. Configuration

### appsettings.json (Development)

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=AdvisorySystemDB;..."
  },
  "Jwt": {
    "Issuer": "AdvisorySystem",
    "Audience": "AdvisorySystem",
    "Key": "<secret>",
    "ExpiresMinutes": 1440
  },
  "Storage": {
    "Root": "wwwroot/uploads",
    "MaxFileSize": 10485760
  },
  "Azure": {
    "StorageConnectionString": "",
    "ContainerName": "documents",
    "ApplicationInsights": { "ConnectionString": "" }
  }
}
```

### Environment Variables (Production — Azure App Service)
Sensitive values are injected as App Service environment variables, not stored in source:

| Variable | Description |
|---|---|
| `ConnectionStrings__Default` | Azure SQL connection string |
| `Jwt__Key` | JWT signing secret |
| `Azure__StorageConnectionString` | Blob Storage connection |
| `Azure__ApplicationInsights__ConnectionString` | App Insights key |

> The app throws `InvalidOperationException` on startup if `Jwt__Key` is missing.

### CORS
- **Development:** `localhost:5173`, `5174`, `5175`, `3000`, `44375`
- **Production:** `https://nice-sand-008811f03.7.azurestaticapps.net` only

---

*Last updated: June 2025*
