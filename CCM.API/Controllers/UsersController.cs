using CCM.API.Data; using CCM.API.DTOs; using CCM.API.Models; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
namespace CCM.API.Controllers;
[ApiController][Route("api/users")][Authorize]
public class UsersController : ControllerBase {
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db){_db=db;}
    [HttpGet] public async Task<IActionResult> GetAll() {
        if(User.FindFirst("role")?.Value!="admin") return Forbid();
        return Ok(await _db.Users.Select(u=>new{u.Id,u.Username,u.FullName,u.Email,u.Role,u.IsActive,u.CreatedAt,u.LastLoginAt}).ToListAsync());
    }
    [HttpPost] public async Task<IActionResult> Create([FromBody] CreateUserDto dto) {
        if(User.FindFirst("role")?.Value!="admin") return Forbid();
        if(await _db.Users.AnyAsync(u=>u.Username==dto.Username)) return BadRequest(new{error="Username exists."});
        var u=new User{Username=dto.Username,PasswordHash=BCrypt.Net.BCrypt.HashPassword(dto.Password),FullName=dto.FullName,Email=dto.Email,Role=dto.Role};
        _db.Users.Add(u); await _db.SaveChangesAsync(); return Ok(new{u.Id,u.Username,u.Role});
    }
    [HttpPut("{id}")] public async Task<IActionResult> Update(int id,[FromBody] UpdateUserDto dto) {
        if(User.FindFirst("role")?.Value!="admin") return Forbid();
        var u=await _db.Users.FindAsync(id); if(u==null) return NotFound();
        u.FullName=dto.FullName; u.Email=dto.Email; u.Role=dto.Role; u.IsActive=dto.IsActive;
        if(!string.IsNullOrEmpty(dto.NewPassword)) u.PasswordHash=BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync(); return Ok(new{u.Id,u.Role,u.IsActive});
    }
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id) {
        if(User.FindFirst("role")?.Value!="admin") return Forbid();
        var u=await _db.Users.FindAsync(id); if(u==null) return NotFound();
        u.IsActive=false; await _db.SaveChangesAsync(); return Ok();
    }
}