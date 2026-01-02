-- EF Core vs SQL Karþýlaþtýrma Testi
-- Bu sorgularý sýrayla çalýþtýrýn

USE AdvisorySystemDB;
GO

-- 1. SQL ile veri var mý?
PRINT '==========================================';
PRINT '1. SQL Seviyesi - Raw Data';
PRINT '==========================================';
SELECT TOP 3
    Id,
    CourseCode,
    CourseName,
    Description,
    LEN(Description) as DescLength,
  DATALENGTH(Description) as DataBytes,
    CASE 
        WHEN Description IS NULL THEN 'NULL'
    WHEN Description = '' THEN 'EMPTY'
        ELSE 'HAS DATA'
    END as Status
FROM Courses
ORDER BY Id;
GO

-- 2. Encoding testi
PRINT '';
PRINT '==========================================';
PRINT '2. Encoding Test';
PRINT '==========================================';
SELECT TOP 1
    CourseCode,
    Description,
    CONVERT(VARBINARY(100), Description) as HexData,
 UNICODE(SUBSTRING(Description, 1, 1)) as FirstCharUnicode
FROM Courses
WHERE Description IS NOT NULL
ORDER BY Id;
GO

-- 3. Collation testi
PRINT '';
PRINT '==========================================';
PRINT '3. Collation Test';
PRINT '==========================================';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    COLLATION_NAME,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Courses' 
  AND COLUMN_NAME IN ('CourseCode', 'CourseName', 'Description');
GO

-- 4. Tüm derslerin durumu
PRINT '';
PRINT '==========================================';
PRINT '4. Tüm Dersler Özet';
PRINT '==========================================';
SELECT 
    COUNT(*) as TotalCourses,
    SUM(CASE WHEN Description IS NULL THEN 1 ELSE 0 END) as NullCount,
    SUM(CASE WHEN Description = '' THEN 1 ELSE 0 END) as EmptyCount,
    SUM(CASE WHEN Description IS NOT NULL AND Description != '' THEN 1 ELSE 0 END) as HasDataCount,
    AVG(LEN(Description)) as AvgLength,
    MIN(LEN(Description)) as MinLength,
    MAX(LEN(Description)) as MaxLength
FROM Courses;
GO

-- 5. Problematic kayýtlar var mý?
PRINT '';
PRINT '==========================================';
PRINT '5. Sorunlu Kayýtlar';
PRINT '==========================================';
SELECT 
    Id,
    CourseCode,
    CASE 
        WHEN Description IS NULL THEN 'IS NULL'
        WHEN Description = '' THEN 'IS EMPTY STRING'
     WHEN LEN(Description) = 0 THEN 'LENGTH ZERO BUT NOT NULL'
        WHEN DATALENGTH(Description) = 0 THEN 'DATALENGTH ZERO'
        ELSE 'OK'
  END as Issue,
    LEN(Description) as Len,
    DATALENGTH(Description) as DataLen
FROM Courses
WHERE Description IS NULL 
   OR Description = '' 
   OR LEN(Description) = 0
   OR DATALENGTH(Description) = 0;
GO

-- 6. Unicode karakter testi (Türkçe)
PRINT '';
PRINT '==========================================';
PRINT '6. Türkçe Karakter Testi';
PRINT '==========================================';
SELECT TOP 5
    CourseCode,
    CourseName,
    LEFT(Description, 80) as DescriptionPreview,
    CASE 
    WHEN CourseName LIKE '%[ÝÞÐÜÇÖýüðþçö]%' THEN 'Türkçe karakter var'
ELSE 'Türkçe karakter yok'
    END as TurkishCharStatus
FROM Courses
WHERE CourseName LIKE '%Ý%' OR CourseName LIKE '%Þ%'
ORDER BY CourseCode;
GO

PRINT '';
PRINT '==========================================';
PRINT 'TEST TAMAMLANDI';
PRINT '==========================================';
PRINT '';
PRINT 'Þimdi uygulamayý çalýþtýrýn ve /api/courses/diagnostics endpoint''ini kontrol edin';
PRINT 'rawDataTest field''ýnda EF Core''un okuduðu veriyi göreceksiniz';
GO
