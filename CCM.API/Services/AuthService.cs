using CCM.API.Data; using CCM.API.Helpers; using CCM.API.Models; using Microsoft.EntityFrameworkCore;
namespace CCM.API.Services;
public class AuthService {
    private readonly AppDbContext _db; private readonly IConfiguration _cfg;
    public AuthService(AppDbContext db, IConfiguration cfg){_db=db;_cfg=cfg;}
    public async Task<(bool,string,User?)> LoginAsync(string u, string p) {
        var user = await _db.Users.FirstOrDefaultAsync(x=>x.Username==u&&x.IsActive);
        if(user==null||!BCrypt.Net.BCrypt.Verify(p,user.PasswordHash)) return(false,"",null);
        user.LastLoginAt=DateTime.UtcNow; await _db.SaveChangesAsync();
        return(true,JwtHelper.GenerateToken(user,_cfg),user);
    }
}