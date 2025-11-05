using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AdvisorySystem.Api.Data
{
 // Design-time factory so EF tools can create AppDbContext without application host
 public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
 {
 public AppDbContext CreateDbContext(string[] args)
 {
 var builder = new DbContextOptionsBuilder<AppDbContext>();
 // Use the same connection string as appsettings.json
 builder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=AdvisorySystemDB;Trusted_Connection=True;TrustServerCertificate=True");
 return new AppDbContext(builder.Options);
 }
 }
}
