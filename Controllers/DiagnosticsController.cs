using AdvisorySystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AdvisorySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("admin")]
public class DiagnosticsController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(UserManager<AppUser> userManager, ILogger<DiagnosticsController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet("advisor-assignments")]
    public async Task<IActionResult> GetAdvisorAssignments()
    {
        try
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var assignments = new List<object>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
 
                assignments.Add(new
                {
                    userId = user.Id,
                    email = user.Email,
                    userName = user.UserName,
                    roles = roles.ToList(),
                    advisorId = user.AdvisorId,
                    hasAdvisor = !string.IsNullOrEmpty(user.AdvisorId)
                });
            }

            var students = assignments.Where(a => ((dynamic)a).roles.Contains("Student")).ToList();
            var advisors = assignments.Where(a => ((dynamic)a).roles.Contains("Advisor")).ToList();

            return Ok(new
            {
                totalUsers = allUsers.Count,
                totalStudents = students.Count,
                totalAdvisors = advisors.Count,
                studentsWithAdvisor = students.Count(s => ((dynamic)s).hasAdvisor),
                studentsWithoutAdvisor = students.Count(s => !((dynamic)s).hasAdvisor),
                allAssignments = assignments,
                studentsByAdvisor = students
                    .Where(s => ((dynamic)s).hasAdvisor)
                    .GroupBy(s => ((dynamic)s).advisorId)
                    .Select(g => new
                    {
                        advisorId = g.Key,
                        advisorEmail = advisors.FirstOrDefault(a => ((dynamic)a).userId == g.Key)?.GetType()
                            .GetProperty("email")?.GetValue(advisors.FirstOrDefault(a => ((dynamic)a).userId == g.Key)),
                        studentCount = g.Count(),
                        students = g.Select(s => new
                        {
                            studentId = ((dynamic)s).userId,
                            studentEmail = ((dynamic)s).email
                        }).ToList()
                    })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get advisor assignments");
            return StatusCode(500, new { error = "Failed to retrieve assignments", details = ex.Message });
        }
    }
}
