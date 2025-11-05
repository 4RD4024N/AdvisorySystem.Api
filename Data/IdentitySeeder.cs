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

        string[] roles = new[] { "Student", "Advisor", "Admin" };
        foreach (var r in roles)
            if (!await roleMgr.RoleExistsAsync(r))
                await roleMgr.CreateAsync(new IdentityRole(r));

        // İsteğe bağlı: admin oluştur
        var adminEmail = "admin@local";
        var admin = await userMgr.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new AppUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userMgr.CreateAsync(admin, "Admin123!");
            await userMgr.AddToRoleAsync(admin, "Admin");
        }

        // Öğrenci oluştur
        var studentEmail = "stu@local";
        var student = await userMgr.FindByEmailAsync(studentEmail);
        if (student is null)
        {
            student = new AppUser { UserName = studentEmail, Email = studentEmail, EmailConfirmed = true };
            await userMgr.CreateAsync(student, "Arda123!");
            await userMgr.AddToRoleAsync(student, "Student");
        }
    }
}
