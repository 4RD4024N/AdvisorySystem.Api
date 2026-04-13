using AdvisorySystem.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("standard")]
public class CoursesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(AppDbContext db, ILogger<CoursesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCourses(
        [FromQuery] int? categoryId = null,
        [FromQuery] int? semester = null,
      [FromQuery] bool? isElective = null,
        [FromQuery] string? search = null)
{
        try
        {
        var query = _db.Courses.Include(c => c.Category).AsQueryable();

     if (categoryId.HasValue)
     query = query.Where(c => c.CategoryId == categoryId.Value);

            if (semester.HasValue)
     query = query.Where(c => c.Semester == semester.Value);

 if (isElective.HasValue)
        query = query.Where(c => c.IsElective == isElective.Value);

     if (!string.IsNullOrWhiteSpace(search))
  {
   search = search.ToLower();
          query = query.Where(c =>
         c.CourseCode.ToLower().Contains(search) ||
      c.CourseName.ToLower().Contains(search));
            }

  var courses = await query
       .OrderBy(c => c.Category.DisplayOrder)
        .ThenBy(c => c.Semester)
     .ThenBy(c => c.CourseCode)
        .Select(c => new
      {
    c.Id,
      c.CourseCode,
 c.CourseName,
            c.TheoryHours,
       c.PracticeHours,
              c.Credits,
   c.ECTS,
            c.Semester,
          c.IsElective,
   c.Description,
  category = new
               {
c.Category.Id,
       c.Category.Name,
       c.Category.DisplayOrder
            }
                })
  .ToListAsync();

         return Ok(new
            {
            totalCount = courses.Count,
       courses
      });
        }
  catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to get courses");
            return StatusCode(500, new { error = "Failed to retrieve courses", details = ex.Message });
}
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourseById(int id)
    {
   try
  {
            var course = await _db.Courses
             .Include(c => c.Category)
    .Where(c => c.Id == id)
    .Select(c => new
                {
         c.Id,
      c.CourseCode,
               c.CourseName,
        c.TheoryHours,
       c.PracticeHours,
         c.Credits,
    c.ECTS,
       c.Semester,
       c.IsElective,
            c.Description,
       category = new
         {
           c.Category.Id,
   c.Category.Name
           },
   prerequisites = _db.Prerequisites
     .Where(p => p.CourseId == id)
 .Select(p => new
              {
         p.Id,
         p.PrerequisiteCourseId,
          courseCode = _db.Courses
            .Where(c2 => c2.Id == p.PrerequisiteCourseId)
      .Select(c2 => c2.CourseCode)
    .FirstOrDefault(),
     courseName = _db.Courses
    .Where(c2 => c2.Id == p.PrerequisiteCourseId)
       .Select(c2 => c2.CourseName)
         .FirstOrDefault(),
        p.IsMandatory
   })
 .ToList()
       })
 .FirstOrDefaultAsync();

  if (course == null)
                return NotFound(new { error = "Course not found" });

        return Ok(course);
        }
        catch (Exception ex)
     {
   _logger.LogError(ex, "Failed to get course");
   return StatusCode(500, new { error = "Failed to retrieve course", details = ex.Message });
        }
    }

    // ?? TEST ENDPOINT - EF Core RAW DATA
    [HttpGet("test/raw/{id}")]
    [Authorize(Roles = "Admin")]  // AllowAnonymous kaldýrýldý — production'da güvensiz
    public async Task<IActionResult> GetCourseRawTest(int id)
    {
        try
     {
       // Direct entity fetch (no projection)
            var courseEntity = await _db.Courses
       .Include(c => c.Category)
           .FirstOrDefaultAsync(c => c.Id == id);

          if (courseEntity == null)
  return NotFound(new { error = "Course not found" });

  // Return raw entity
            return Ok(new
       {
        message = "RAW EF CORE DATA TEST",
      entity = courseEntity,
         descriptionTests = new
   {
        isNull = courseEntity.Description == null,
     isEmpty = courseEntity.Description == "",
             length = courseEntity.Description?.Length ?? 0,
             hasData = !string.IsNullOrEmpty(courseEntity.Description),
  preview = courseEntity.Description?.Substring(0, Math.Min(100, courseEntity.Description?.Length ?? 0))
             }
   });
        }
 catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get course raw test");
            return StatusCode(500, new { error = "Failed to retrieve course", details = ex.Message });
    }
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _db.CourseCategories
        .OrderBy(c => c.DisplayOrder)
    .Select(c => new
     {
        c.Id,
 c.Name,
  c.Description,
       c.DisplayOrder,
            courseCount = _db.Courses.Count(course => course.CategoryId == c.Id)
          })
    .ToListAsync();

         return Ok(categories);
        }
    catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get categories");
            return StatusCode(500, new { error = "Failed to retrieve categories", details = ex.Message });
      }
    }

    [HttpGet("by-semester/{semester}")]
    public async Task<IActionResult> GetCoursesBySemester(int semester)
    {   
 try
    {
            var courses = await _db.Courses
         .Include(c => c.Category)
            .Where(c => c.Semester == semester)
    .OrderBy(c => c.CourseCode)
           .Select(c => new
           {
                    c.Id,
        c.CourseCode,
       c.CourseName,
      c.TheoryHours,
    c.PracticeHours,
    c.Credits,
           c.ECTS,
 c.IsElective,
           category = c.Category.Name
     })
             .ToListAsync();

     var totalCredits = courses.Where(c => !c.IsElective).Sum(c => c.Credits);
    var totalECTS = courses.Where(c => !c.IsElective).Sum(c => c.ECTS);

         return Ok(new
     {
     semester,
       totalCourses = courses.Count,
           requiredCourses = courses.Count(c => !c.IsElective),
          electiveCourses = courses.Count(c => c.IsElective),
 totalCredits,
             totalECTS,
        courses
        });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get semester courses");
            return StatusCode(500, new { error = "Failed to retrieve courses", details = ex.Message });
   }
    }

    [HttpGet("electives")]
    public async Task<IActionResult> GetElectiveCourses()
    {
        try
        {
   var electives = await _db.Courses
     .Include(c => c.Category)
      .Where(c => c.IsElective)
                .OrderBy(c => c.Category.DisplayOrder)
    .ThenBy(c => c.CourseCode)
  .Select(c => new
     {
         c.Id,
        c.CourseCode,
   c.CourseName,
        c.TheoryHours,
     c.PracticeHours,
             c.Credits,
      c.ECTS,
       category = c.Category.Name
  })
             .ToListAsync();

return Ok(new
            {
       totalElectives = electives.Count,
 electives
            });
}
    catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get elective courses");
       return StatusCode(500, new { error = "Failed to retrieve elective courses", details = ex.Message });
      }
}

 [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
    {
      try
        {
        if (await _db.Courses.AnyAsync(c => c.CourseCode == dto.CourseCode))
        return BadRequest(new { error = "Course code already exists" });

 var course = new Course
          {
  CourseCode = dto.CourseCode,
  CourseName = dto.CourseName,
                TheoryHours = dto.TheoryHours,
       PracticeHours = dto.PracticeHours,
       Credits = dto.Credits,
         ECTS = dto.ECTS,
    CategoryId = dto.CategoryId,
      Semester = dto.Semester,
   IsElective = dto.IsElective,
      Description = dto.Description
  };

         _db.Courses.Add(course);
            await _db.SaveChangesAsync();

 return Ok(new
     {
  message = "Course created successfully",
                courseId = course.Id
      });
        }
        catch (Exception ex)
        {
   _logger.LogError(ex, "Failed to create course");
   return StatusCode(500, new { error = "Failed to create course", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] CreateCourseDto dto)
    {
        try
   {
            var course = await _db.Courses.FindAsync(id);
            if (course == null)
    return NotFound(new { error = "Course not found" });

            if (course.CourseCode != dto.CourseCode &&
          await _db.Courses.AnyAsync(c => c.CourseCode == dto.CourseCode))
      return BadRequest(new { error = "Course code already exists" });

     course.CourseCode = dto.CourseCode;
      course.CourseName = dto.CourseName;
            course.TheoryHours = dto.TheoryHours;
            course.PracticeHours = dto.PracticeHours;
   course.Credits = dto.Credits;
      course.ECTS = dto.ECTS;
            course.CategoryId = dto.CategoryId;
    course.Semester = dto.Semester;
        course.IsElective = dto.IsElective;
    course.Description = dto.Description;

  await _db.SaveChangesAsync();

            return Ok(new { message = "Course updated successfully" });
        }
catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update course");
            return StatusCode(500, new { error = "Failed to update course", details = ex.Message });
      }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        try
        {
 var course = await _db.Courses.FindAsync(id);
            if (course == null)
        return NotFound(new { error = "Course not found" });

       _db.Courses.Remove(course);
         await _db.SaveChangesAsync();

      return Ok(new { message = "Course deleted successfully" });
     }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to delete course");
  return StatusCode(500, new { error = "Failed to delete course", details = ex.Message });
        }
 }

    public record CreateCourseDto(
      string CourseCode,
        string CourseName,
        int TheoryHours,
        int PracticeHours,
        int Credits,
 int ECTS,
        int CategoryId,
        int? Semester,
  bool IsElective,
        string? Description
    );

    [HttpGet("diagnostics")]
    [Authorize(Roles = "Admin")]  // AllowAnonymous kaldýrýldý — production'da güvensiz
    public async Task<IActionResult> GetDiagnostics()
    {
        try
        {
            var totalCourses = await _db.Courses.CountAsync();
            var totalCategories = await _db.CourseCategories.CountAsync();
   
     var categoriesWithCounts = await _db.CourseCategories
         .Select(c => new
          {
    c.Id,
          c.Name,
      c.DisplayOrder,
 CourseCount = _db.Courses.Count(course => course.CategoryId == c.Id)
       })
     .OrderBy(c => c.DisplayOrder)
          .ToListAsync();

        var coursesBySemester = await _db.Courses
     .GroupBy(c => c.Semester)
      .Select(g => new
                {
        Semester = g.Key,
             Count = g.Count(),
   RequiredCount = g.Count(c => !c.IsElective),
 ElectiveCount = g.Count(c => c.IsElective)
  })
      .OrderBy(g => g.Semester)
    .ToListAsync();

            var electiveCourses = await _db.Courses
    .Where(c => c.IsElective)
        .CountAsync();

   var requiredCourses = await _db.Courses
        .Where(c => !c.IsElective)
                .CountAsync();

         // ? Açýklamalarý kontrol et
    var coursesWithDescriptions = await _db.Courses
      .Where(c => c.Description != null && c.Description != "")
   .CountAsync();

 var coursesWithoutDescriptions = await _db.Courses
    .Where(c => c.Description == null || c.Description == "")
       .CountAsync();

            // ?? EF Core ile gerçek veriyi çek (RAW DATA TEST)
   var rawDataTest = await _db.Courses
      .OrderBy(c => c.CourseCode)
      .Take(3)
     .Select(c => new
     {
   c.Id,
        c.CourseCode,
   c.CourseName,
        Description = c.Description, // Direct mapping
     DescriptionLength = c.Description != null ? c.Description.Length : 0,
      DescriptionPreview = c.Description != null ? c.Description.Substring(0, Math.Min(50, c.Description.Length)) : "NULL",
           IsNull = c.Description == null,
 IsEmpty = c.Description == "",
            HasData = c.Description != null && c.Description != ""
                })
.ToListAsync();

         // Sample courses with descriptions
   var sampleCoursesWithDetails = await _db.Courses
           .Include(c => c.Category)
           .OrderBy(c => c.CourseCode)
         .Take(5)
        .Select(c => new
            {
           c.CourseCode,
        c.CourseName,
        c.Semester,
       c.IsElective,
       c.Description,
            hasDescription = !string.IsNullOrEmpty(c.Description),
        descriptionLength = c.Description != null ? c.Description.Length : 0,
              category = c.Category.Name
  })
          .ToListAsync();

         return Ok(new
  {
              summary = new
              {
            totalCourses,
  totalCategories,
            electiveCourses,
   requiredCourses,
      coursesWithDescriptions,
            coursesWithoutDescriptions,
          descriptionCoverage = totalCourses > 0 
  ? Math.Round((double)coursesWithDescriptions / totalCourses * 100, 2) 
          : 0
                },
 rawDataTest, // ?? Bu çok önemli - EF'in gerçekten ne okuduðunu gösterir
           categoriesWithCounts,
     coursesBySemester,
    sampleCoursesWithDetails,
     message = totalCourses == 0 
     ? "?? NO COURSES FOUND! Database needs seeding." 
       : coursesWithDescriptions == 0
            ? $"?? Database has {totalCourses} courses but NO descriptions! Please reseed database."
     : coursesWithDescriptions < totalCourses
      ? $"?? Only {coursesWithDescriptions}/{totalCourses} courses have descriptions!"
   : $"? Database has {totalCourses} courses with {coursesWithDescriptions} descriptions ({Math.Round((double)coursesWithDescriptions / totalCourses * 100, 2)}% coverage)"
            });
        }
        catch (Exception ex)
        {
        _logger.LogError(ex, "Failed to get diagnostics");
      return StatusCode(500, new { error = "Failed to get diagnostics", details = ex.Message });
        }
    }

}
