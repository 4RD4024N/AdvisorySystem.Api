using AdvisorySystem.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AdvisorySystem.Api.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
     using var scope = sp.CreateScope();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        // Rolleri oluştur
        string[] roles = new[] { "Student", "Advisor", "Admin" };
        foreach (var r in roles)
if (!await roleMgr.RoleExistsAsync(r))
            await roleMgr.CreateAsync(new IdentityRole(r));

       
  var adminEmail = "admin@local";
     var admin = await userMgr.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
     admin = new AppUser 
         { 
     UserName = adminEmail, 
   Email = adminEmail, 
       EmailConfirmed = true 
            };
          await userMgr.CreateAsync(admin, "Admin123!");
            await userMgr.AddToRoleAsync(admin, "Admin");
        }

       
 var advisors = new[]
        {
    new { Email = "advisor1@local", Password = "Advisor123!", Name = "Prof. Dr. Ahmet Yilmaz" },
            new { Email = "advisor2@local", Password = "Advisor123!", Name = "Prof. Dr. Ayse Demir" },
    new { Email = "advisor3@local", Password = "Advisor123!", Name = "Doc. Dr. Mehmet Kaya" }
        };

        foreach (var advisorData in advisors)
        {
 var advisor = await userMgr.FindByEmailAsync(advisorData.Email);
            if (advisor is null)
            {
      advisor = new AppUser 
      { 
   UserName = advisorData.Email, 
       Email = advisorData.Email, 
      EmailConfirmed = true 
     };
                await userMgr.CreateAsync(advisor, advisorData.Password);
   await userMgr.AddToRoleAsync(advisor, "Advisor");
    }
        }

        // 3. 3 Student oluştur
 var students = new[]
        {
            new { Email = "student1@local", Password = "Student123!", Name = "Ali Veli" },
     new { Email = "student2@local", Password = "Student123!", Name = "Fatma Yildiz" },
     new { Email = "student3@local", Password = "Student123!", Name = "Can Ozturk" }
        };

        foreach (var studentData in students)
        {
            var student = await userMgr.FindByEmailAsync(studentData.Email);
            if (student is null)
        {
    student = new AppUser 
       { 
 UserName = studentData.Email, 
        Email = studentData.Email, 
          EmailConfirmed = true,
    AdvisorId = null  // Başlangıçta advisor atanmamış
     };
       await userMgr.CreateAsync(student, studentData.Password);
       await userMgr.AddToRoleAsync(student, "Student");
            }
   }
    }
}
