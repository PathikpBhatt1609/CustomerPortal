using CCM.API.Models;
namespace CCM.API.Data;
public static class DbSeeder {
    public static async Task SeedAsync(AppDbContext db) {
        if (!db.Users.Any()) {
            db.Users.AddRange(
                new User{Username="requestor",PasswordHash=BCrypt.Net.BCrypt.HashPassword("pass123"),FullName="Rahul Mehta",Email="rahul@company.com",Role="requestor"},
                new User{Username="teamlead",PasswordHash=BCrypt.Net.BCrypt.HashPassword("pass123"),FullName="Priya Sharma",Email="priya@company.com",Role="teamlead"},
                new User{Username="manager",PasswordHash=BCrypt.Net.BCrypt.HashPassword("pass123"),FullName="Anil Verma",Email="anil@company.com",Role="manager"},
                new User{Username="admin",PasswordHash=BCrypt.Net.BCrypt.HashPassword("pass123"),FullName="Admin User",Email="admin@company.com",Role="admin"}
            );
            await db.SaveChangesAsync();
        }
        if (!db.SapConfigs.Any()) {
            db.SapConfigs.Add(new SapConfig{BaseUrl="https://sap.yourcompany.com",Mode="mock"});
            await db.SaveChangesAsync();
        }
    }
}