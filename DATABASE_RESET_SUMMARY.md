# ?? Database Reset & New Seed Data

**Date:** December 20, 2024  
**Action:** Database dropped and recreated with new seed data  
**Status:** ? COMPLETED

---

## ??? Database Reset

### Commands Executed:
```bash
# 1. Stop IIS Express
taskkill /F /IM "iisexpress.exe"

# 2. Build project
dotnet build

# 3. Drop database
dotnet ef database drop --force
# Result: Successfully dropped database 'AdvisorySystemDB'

# 4. Recreate database
dotnet ef database update
# Applied migrations:
# - 20251115160705_InitialCreate
# - 20251117181312_AddStudentProfileAndRatingFeatures
# - 20251118160801_UpdateSubmissionAndFileValidation
# - 20251220160037_AddStudentAdvisorRelationship
```

---

## ?? New Seed Data

### Updated Configuration:

**Before:**
- 1 Admin (admin@local)
- 1 Advisor (ad@local)
- 1 Student (stu@local)

**After:**
- ? 1 Admin
- ? 3 Advisors
- ? 3 Students

---

## ?? Seed Users

### 1. Admin (1 user)

| Email | Password | Role | Status |
|-------|----------|------|--------|
| admin@local | Admin123! | Admin | ? Confirmed |

**Purpose:** System administration, advisor assignment

---

### 2. Advisors (3 users)

| Email | Password | Role | Name | Status |
|-------|----------|------|------|--------|
| advisor1@local | Advisor123! | Advisor | Prof. Dr. Ahmet Yýlmaz | ? Confirmed |
| advisor2@local | Advisor123! | Advisor | Prof. Dr. Ayþe Demir | ? Confirmed |
| advisor3@local | Advisor123! | Advisor | Doç. Dr. Mehmet Kaya | ? Confirmed |

**Purpose:** 
- Can be assigned to students
- Can view assigned students
- Can provide feedback

---

### 3. Students (3 users)

| Email | Password | Role | Name | Advisor | Status |
|-------|----------|------|------|---------|--------|
| student1@local | Student123! | Student | Ali Veli | None | ? Confirmed |
| student2@local | Student123! | Student | Fatma Yýldýz | None | ? Confirmed |
| student3@local | Student123! | Student | Can Öztürk | None | ? Confirmed |

**Initial State:** All students have `AdvisorId = NULL` (no advisor assigned)

**Purpose:**
- Create documents
- Upload versions
- Receive assignments
- Can be assigned to advisors by admin

---

## ?? Implementation Details

### IdentitySeeder.cs Changes:

```csharp
public static async Task SeedAsync(IServiceProvider sp)
{
 // ... role creation ...

 // 1. Admin oluþtur
    var admin = new AppUser 
    { 
        UserName = "admin@local", 
    Email = "admin@local", 
        EmailConfirmed = true 
    };
    await userMgr.CreateAsync(admin, "Admin123!");
    await userMgr.AddToRoleAsync(admin, "Admin");

    // 2. 3 Advisor oluþtur
 var advisors = new[]
    {
        new { Email = "advisor1@local", Password = "Advisor123!", Name = "Prof. Dr. Ahmet Yýlmaz" },
        new { Email = "advisor2@local", Password = "Advisor123!", Name = "Prof. Dr. Ayþe Demir" },
        new { Email = "advisor3@local", Password = "Advisor123!", Name = "Doç. Dr. Mehmet Kaya" }
    };

    foreach (var advisorData in advisors)
    {
     var advisor = new AppUser 
  { 
     UserName = advisorData.Email, 
        Email = advisorData.Email, 
EmailConfirmed = true 
    };
        await userMgr.CreateAsync(advisor, advisorData.Password);
     await userMgr.AddToRoleAsync(advisor, "Advisor");
    }

    // 3. 3 Student oluþtur
 var students = new[]
    {
        new { Email = "student1@local", Password = "Student123!", Name = "Ali Veli" },
        new { Email = "student2@local", Password = "Student123!", Name = "Fatma Yýldýz" },
        new { Email = "student3@local", Password = "Student123!", Name = "Can Öztürk" }
    };

    foreach (var studentData in students)
    {
        var student = new AppUser 
        { 
            UserName = studentData.Email, 
            Email = studentData.Email, 
            EmailConfirmed = true,
    AdvisorId = null  // Baþlangýçta advisor atanmamýþ
        };
 await userMgr.CreateAsync(student, studentData.Password);
    await userMgr.AddToRoleAsync(student, "Student");
    }
}
```

---

## ?? Testing the Seed Data

### 1. Login as Admin
```bash
curl -X POST https://localhost:7175/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@local",
    "password": "Admin123!"
  }'
```

### 2. Get All Advisors
```bash
curl -X GET https://localhost:7175/api/advisors \
  -H "Authorization: Bearer {token}"
```

**Expected Response:**
```json
{
  "totalAdvisors": 3,
  "advisors": [
    {
      "id": "...",
      "userName": "advisor1@local",
   "email": "advisor1@local",
      "emailConfirmed": true
    },
 {
      "id": "...",
  "userName": "advisor2@local",
      "email": "advisor2@local",
      "emailConfirmed": true
    },
    {
      "id": "...",
      "userName": "advisor3@local",
"email": "advisor3@local",
      "emailConfirmed": true
    }
  ]
}
```

### 3. Get All Students
```bash
curl -X GET https://localhost:7175/api/students \
  -H "Authorization: Bearer {token}"
```

**Expected Response:**
```json
{
  "totalCount": 3,
  "students": [
    {
      "id": "...",
      "userName": "student1@local",
  "email": "student1@local",
      "hasAdvisor": false,
      "advisor": null
    },
    {
      "id": "...",
      "userName": "student2@local",
      "email": "student2@local",
      "hasAdvisor": false,
      "advisor": null
    },
    {
      "id": "...",
      "userName": "student3@local",
      "email": "student3@local",
      "hasAdvisor": false,
      "advisor": null
    }
  ]
}
```

### 4. Assign Advisor to Student
```bash
curl -X POST https://localhost:7175/api/advisors/assign \
  -H "Authorization: Bearer {admin-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "studentId": "{student1-id}",
    "advisorId": "{advisor1-id}"
  }'
```

---

## ?? Use Cases

### Scenario 1: Admin Assigns All Students to Advisors
```
student1@local ? advisor1@local (Prof. Dr. Ahmet Yýlmaz)
student2@local ? advisor2@local (Prof. Dr. Ayþe Demir)
student3@local ? advisor3@local (Doç. Dr. Mehmet Kaya)
```

### Scenario 2: One Advisor for Multiple Students
```
advisor1@local has:
  - student1@local
  - student2@local
  - student3@local
```

### Scenario 3: Mix of Assigned and Unassigned
```
student1@local ? advisor1@local (assigned)
student2@local ? advisor2@local (assigned)
student3@local ? NULL (waiting for assignment)
```

---

## ?? Admin Panel Testing

### Expected UI State on Startup:

**Statistics:**
- ?? Total Students: 3
- ? Assigned: 0
- ?? Unassigned: 3
- ????? Total Advisors: 3

**Student List:**
```
??????????????????????????????????????????????????????????????????????????
? Student        ? Email       ? Status ? Advisor     ? Actions  ?
??????????????????????????????????????????????????????????????????????????
? student1@local   ? student1@local    ? ?? No  ? -           ? [Assign] ?
? student2@local   ? student2@local    ? ?? No  ? -           ? [Assign] ?
? student3@local   ? student3@local    ? ?? No  ? -           ? [Assign] ?
??????????????????????????????????????????????????????????????????????????
```

---

## ?? Login Credentials Summary

### Quick Reference:

**Admin:**
```
Username: admin@local
Password: Admin123!
```

**Advisors:**
```
1. advisor1@local / Advisor123!
2. advisor2@local / Advisor123!
3. advisor3@local / Advisor123!
```

**Students:**
```
1. student1@local / Student123!
2. student2@local / Student123!
3. student3@local / Student123!
```

---

## ? Verification Checklist

- [x] Database dropped successfully
- [x] Migrations applied
- [x] Seed data configured (1 admin, 3 advisors, 3 students)
- [x] Build successful
- [ ] Application started
- [ ] Admin login tested
- [ ] Advisors list verified (should show 3)
- [ ] Students list verified (should show 3, all unassigned)
- [ ] Advisor assignment tested

---

## ?? Next Steps

1. **Start Application:**
```bash
dotnet run
```

2. **Open Swagger:**
```
https://localhost:7175/swagger
```

3. **Login as Admin:**
- Use `/api/auth/login` with `admin@local` / `Admin123!`

4. **Verify Seed Data:**
- Check `/api/advisors` ? should return 3 advisors
- Check `/api/students` ? should return 3 students
- Check `/api/students/without-advisor` ? should return 3 students

5. **Test Assignment:**
- Use admin panel to assign advisors to students
- Verify notifications sent
- Check `GET /api/students` includes advisor info

---

## ?? Database State

### AspNetUsers Table (Expected):

```sql
SELECT 
u.Id,
    u.UserName,
u.Email,
    r.Name as Role,
    u.AdvisorId
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
ORDER BY r.Name, u.UserName;
```

**Expected Results (7 rows):**
```
Id       | UserName  | Email   | Role    | AdvisorId
--------------------+-------------------+-------------------+---------+-----------
{guid-1}     | admin@local       | admin@local  | Admin   | NULL
{guid-2}    | advisor1@local    | advisor1@local    | Advisor | NULL
{guid-3}       | advisor2@local    | advisor2@local    | Advisor | NULL
{guid-4}            | advisor3@local | advisor3@local    | Advisor | NULL
{guid-5}   | student1@local| student1@local    | Student | NULL
{guid-6}            | student2@local    | student2@local    | Student | NULL
{guid-7}        | student3@local    | student3@local    | Student | NULL
```

---

**Status:** ? Database reset complete, ready for testing  
**Total Users:** 7 (1 admin + 3 advisors + 3 students)
**All Passwords:** {Role}123! format
