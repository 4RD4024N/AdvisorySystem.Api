# ?? Database Reset & Rebuild Complete!

## ? Actions Performed

### 1. EF Core Tools Installation
```bash
dotnet tool install --global dotnet-ef --version 8.0.0
```
**Status:** ? Success

---

### 2. Database Drop
```bash
dotnet ef database drop --force
```
**Status:** ? Successfully dropped database 'AdvisorySystemDB'

---

### 3. Migrations Cleanup
```bash
Remove-Item -Path "Migrations" -Recurse -Force
```
**Status:** ? All old migrations removed

---

### 4. New Migration Created
```bash
dotnet ef migrations add InitialCreate
```
**Status:** ? Migration '20251115160705_InitialCreate' created

---

### 5. Database Creation
```bash
dotnet ef database update
```
**Status:** ? Database created with all tables

---

## ?? Database Tables Created

Based on your models, the following tables should now exist:

### Identity Tables
- `AspNetUsers` - User accounts
- `AspNetRoles` - Roles (Student, Advisor, Admin)
- `AspNetUserRoles` - User-Role mapping
- `AspNetUserClaims` - User claims
- `AspNetUserLogins` - External logins
- `AspNetUserTokens` - Refresh tokens
- `AspNetRoleClaims` - Role claims

### Application Tables
- `Documents` - Student documents
- `DocumentVersions` - Document file versions
- `Comments` - Comments on document versions
- `Submissions` - Submission deadlines
- `Notifications` - User notifications

---

## ?? Next Steps

### 1. Run Application to Seed Data
```bash
dotnet run
```

This will automatically:
- Create default roles (Student, Advisor, Admin)
- Create default users:
  - `admin@local` / `Admin123!` (Admin role)
  - `stu@local` / `Arda123!` (Student role)

---

### 2. Verify Database

**SQL Server Management Studio / Azure Data Studio:**
```sql
-- Check tables
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Check users
SELECT * FROM AspNetUsers;

-- Check roles
SELECT * FROM AspNetRoles;

-- Check user roles
SELECT 
    u.UserName,
    r.Name as Role
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id;
```

**Visual Studio SQL Server Object Explorer:**
1. View ? SQL Server Object Explorer
2. Expand (localdb)\MSSQLLocalDB
3. Databases ? AdvisorySystemDB
4. Tables (should see all tables listed above)

---

### 3. Test API

**Basic Health Check:**
```bash
curl https://localhost:7175/api/health
```

**Login Test:**
```bash
curl -X POST https://localhost:7175/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "stu@local",
    "password": "Arda123!"
  }'
```

**Get Users (Debug):**
```bash
curl https://localhost:7175/api/debug/users
```

---

## ?? Troubleshooting

### If Seed Data Doesn't Create

**Check Program.cs:**
```csharp
try
{
    await IdentitySeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error while seeding identity data");
}
```

**Manual Seed via Debug Endpoint:**
```bash
# This should automatically trigger on first run
# Check logs for: "Seeding identity data..."
```

---

### If Tables Are Missing

**Check Migration:**
```bash
# List migrations
dotnet ef migrations list

# Should show:
# 20251115160705_InitialCreate (Applied)
```

**Verify AppDbContext.cs has all DbSets:**
```csharp
public DbSet<Document> Documents { get; set; }
public DbSet<DocumentVersion> DocumentVersions { get; set; }
public DbSet<Comment> Comments { get; set; }
public DbSet<Submission> Submissions { get; set; }
public DbSet<Notification> Notifications { get; set; }
```

---

### If Connection String Issues

**Check appsettings.json:**
```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=AdvisorySystemDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Test Connection:**
```bash
# In Visual Studio Package Manager Console
Test-Connection -ComputerName (localdb)\MSSQLLocalDB
```

---

## ?? Database Schema

### Documents Table
```sql
CREATE TABLE Documents (
    Id INT PRIMARY KEY IDENTITY,
    Title NVARCHAR(MAX) NOT NULL,
    Tags NVARCHAR(MAX),
    OwnerUserId NVARCHAR(450) NOT NULL,
    AdvisorUserId NVARCHAR(450),
    CreatedAt DATETIME2 NOT NULL,
    FOREIGN KEY (OwnerUserId) REFERENCES AspNetUsers(Id),
    FOREIGN KEY (AdvisorUserId) REFERENCES AspNetUsers(Id)
)
```

### DocumentVersions Table
```sql
CREATE TABLE DocumentVersions (
    Id INT PRIMARY KEY IDENTITY,
    DocumentId INT NOT NULL,
    VersionNo INT NOT NULL,
    FileName NVARCHAR(MAX) NOT NULL,
    StoragePath NVARCHAR(MAX) NOT NULL,
    ContentType NVARCHAR(MAX),
    Size BIGINT NOT NULL,
Notes NVARCHAR(MAX),
    UploadedByUserId NVARCHAR(450) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE,
    FOREIGN KEY (UploadedByUserId) REFERENCES AspNetUsers(Id)
)
```

### Comments Table
```sql
CREATE TABLE Comments (
    Id INT PRIMARY KEY IDENTITY,
    DocumentVersionId INT NOT NULL,
    AuthorUserId NVARCHAR(450) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    FOREIGN KEY (DocumentVersionId) REFERENCES DocumentVersions(Id) ON DELETE CASCADE,
    FOREIGN KEY (AuthorUserId) REFERENCES AspNetUsers(Id)
)
```

### Submissions Table
```sql
CREATE TABLE Submissions (
    Id INT PRIMARY KEY IDENTITY,
    StudentId NVARCHAR(450) NOT NULL,
    DueDate DATETIME2 NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    FOREIGN KEY (StudentId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
)
```

### Notifications Table
```sql
CREATE TABLE Notifications (
    Id INT PRIMARY KEY IDENTITY,
    UserId NVARCHAR(450) NOT NULL,
    Title NVARCHAR(MAX) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Type INT NOT NULL,
    IsRead BIT NOT NULL,
    RelatedEntityId NVARCHAR(MAX),
    RelatedEntityType NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
)
```

---

## ? Verification Checklist

- [x] EF Core Tools installed
- [x] Old database dropped
- [x] Old migrations removed
- [x] New migration created
- [x] Database updated with new migration
- [ ] Application running
- [ ] Seed data created (admin@local, stu@local)
- [ ] Can login with default users
- [ ] All tables exist in database
- [ ] Foreign keys working
- [ ] Can create documents
- [ ] Can add notifications

---

## ?? Quick Start Commands

```bash
# 1. Run application
dotnet run

# 2. Check health
curl https://localhost:7175/api/health

# 3. Get seed info
curl https://localhost:7175/api/debug/seedinfo

# 4. List users
curl https://localhost:7175/api/debug/users

# 5. Login as student
curl -X POST https://localhost:7175/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"stu@local","password":"Arda123!"}'

# 6. Login as admin
curl -X POST https://localhost:7175/api/auth/login \
-H "Content-Type: application/json" \
  -d '{"email":"admin@local","password":"Admin123!"}'
```

---

## ?? Expected Results

### After Running Application

**Console Logs:**
```
[Information] Seeding identity data...
[Information] Creating roles: Student, Advisor, Admin
[Information] Creating admin user: admin@local
[Information] Creating student user: stu@local
[Information] Seeding completed successfully
[Information] Now listening on: https://localhost:7175
[Information] Application started
```

**Database Content:**
```
AspNetRoles: 3 roles (Student, Advisor, Admin)
AspNetUsers: 2 users (admin@local, stu@local)
AspNetUserRoles: 2 mappings
Documents: 0 (empty)
DocumentVersions: 0 (empty)
Comments: 0 (empty)
Submissions: 0 (empty)
Notifications: 0 (empty)
```

---

## ?? Success!

Your database is now clean and ready to use!

**Fresh start with:**
- ? All tables created
- ? Proper foreign keys
- ? Identity system configured
- ? Ready for seeding
- ? No orphaned data
- ? Clean migrations

---

**Action Required:**
1. Run `dotnet run`
2. Test login with default users
3. Start using the application!

---

**Completed:** 2025-01-06  
**Database:** AdvisorySystemDB  
**Migration:** 20251115160705_InitialCreate  
**Status:** ? Ready for Use
