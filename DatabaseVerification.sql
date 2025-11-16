-- Database Reset Verification Queries
-- Run these in SQL Server Management Studio or Azure Data Studio

-- 1. Check all tables
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- 2. Check Identity Tables
SELECT 'AspNetUsers' as TableName, COUNT(*) as RecordCount FROM AspNetUsers
UNION ALL
SELECT 'AspNetRoles', COUNT(*) FROM AspNetRoles
UNION ALL
SELECT 'AspNetUserRoles', COUNT(*) FROM AspNetUserRoles
UNION ALL
SELECT 'AspNetUserClaims', COUNT(*) FROM AspNetUserClaims
UNION ALL
SELECT 'AspNetUserLogins', COUNT(*) FROM AspNetUserLogins
UNION ALL
SELECT 'AspNetUserTokens', COUNT(*) FROM AspNetUserTokens
UNION ALL
SELECT 'AspNetRoleClaims', COUNT(*) FROM AspNetRoleClaims;

-- 3. Check Application Tables
SELECT 'Documents' as TableName, COUNT(*) as RecordCount FROM Documents
UNION ALL
SELECT 'DocumentVersions', COUNT(*) FROM DocumentVersions
UNION ALL
SELECT 'Comments', COUNT(*) FROM Comments
UNION ALL
SELECT 'Submissions', COUNT(*) FROM Submissions
UNION ALL
SELECT 'Notifications', COUNT(*) FROM Notifications;

-- 4. Check Users (after seeding)
SELECT 
    Id,
    UserName,
    Email,
    EmailConfirmed
FROM AspNetUsers;

-- 5. Check Roles (after seeding)
SELECT 
    Id,
    Name,
    NormalizedName
FROM AspNetRoles;

-- 6. Check User Roles (after seeding)
SELECT 
    u.UserName,
    u.Email,
    r.Name as RoleName
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
ORDER BY u.UserName;

-- 7. Check Foreign Keys
SELECT 
    OBJECT_NAME(f.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_NAME(f.referenced_object_id) AS ReferencedTable,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys AS f
INNER JOIN sys.foreign_key_columns AS fc ON f.object_id = fc.constraint_object_id
WHERE OBJECT_NAME(f.parent_object_id) IN ('Documents', 'DocumentVersions', 'Comments', 'Submissions', 'Notifications')
ORDER BY TableName;

-- 8. Check Indexes
SELECT 
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
WHERE OBJECT_NAME(i.object_id) IN ('Documents', 'DocumentVersions', 'Comments', 'Submissions', 'Notifications')
ORDER BY TableName, IndexName;

-- 9. Test Data After Seeding
-- Run after: dotnet run

-- Should return 2 users
SELECT COUNT(*) as TotalUsers FROM AspNetUsers;

-- Should return 3 roles
SELECT COUNT(*) as TotalRoles FROM AspNetRoles;

-- Should return 2 user-role assignments
SELECT COUNT(*) as TotalUserRoles FROM AspNetUserRoles;

-- 10. Verify Default Users
SELECT 
    u.Email,
    STRING_AGG(r.Name, ', ') as Roles
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
GROUP BY u.Email
ORDER BY u.Email;

-- Expected Results:
-- admin@local | Admin
-- stu@local   | Student
