# ? Database Reset - Quick Reference

## ?? What Was Done

```bash
# 1. Installed EF Core Tools
dotnet tool install --global dotnet-ef --version 8.0.0

# 2. Dropped existing database
dotnet ef database drop --force

# 3. Removed old migrations
Remove-Item -Path "Migrations" -Recurse -Force

# 4. Created new migration
dotnet ef migrations add InitialCreate

# 5. Applied migration
dotnet ef database update
```

**Result:** ? Clean database with all tables created

---

## ?? Next: Run Application

```bash
dotnet run
```

This will:
- ? Seed default roles (Student, Advisor, Admin)
- ? Create admin user: `admin@local` / `Admin123!`
- ? Create student user: `stu@local` / `Arda123!`

---

## ?? Quick Tests

### 1. Health Check
```bash
curl https://localhost:7175/api/health
```

### 2. Login as Student
```bash
curl -X POST https://localhost:7175/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"stu@local","password":"Arda123!"}'
```

### 3. Login as Admin
```bash
curl -X POST https://localhost:7175/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@local","password":"Admin123!"}'
```

### 4. List Users
```bash
curl https://localhost:7175/api/debug/users
```

### 5. Get Seed Info
```bash
curl https://localhost:7175/api/debug/seedinfo
```

---

## ?? Database Tables

### Identity (ASP.NET Core Identity)
- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- AspNetUserLogins
- AspNetUserTokens
- AspNetRoleClaims

### Application
- Documents
- DocumentVersions
- Comments
- Submissions
- Notifications

---

## ?? Verify Database

**SQL Query:**
```sql
-- Run in SSMS or Azure Data Studio
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

**Connection String:**
```
Server=(localdb)\MSSQLLocalDB;Database=AdvisorySystemDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

---

## ?? Files Created

1. **DATABASE_RESET_COMPLETE.md** - Full documentation
2. **DatabaseVerification.sql** - SQL verification queries
3. **DATABASE_QUICK_REFERENCE.md** - This file

---

## ?? Important Notes

1. **Seed Data:** Automatically created on first run
2. **Default Users:** Check with `/api/debug/users`
3. **Frontend:** Need to re-login (old tokens invalid)
4. **LocalDB:** Must be running

---

## ?? Status

- ? Database dropped
- ? Migrations cleaned
- ? New migration created
- ? Database recreated
- ? Tables created
- ? Pending: Run application
- ? Pending: Seed data
- ? Pending: Test endpoints

---

**Next Step:** Run `dotnet run` and test!
