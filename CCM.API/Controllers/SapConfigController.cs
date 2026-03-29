using CCM.API.Data; using CCM.API.Models; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
namespace CCM.API.Controllers;
[ApiController][Route("api/sapconfig")][Authorize]
public class SapConfigController : ControllerBase {
    private readonly AppDbContext _db;
    public SapConfigController(AppDbContext db){_db=db;}
    [HttpGet] public async Task<IActionResult> Get() {
        var c=await _db.SapConfigs.FirstOrDefaultAsync(); if(c==null) return NotFound();
        return Ok(new{c.Id,c.BaseUrl,c.ApiPath,c.SapUsername,c.ClientNo,c.AccountGrp,c.Mode,c.TimeoutMs,c.UpdatedAt});
    }
    [HttpPost] public async Task<IActionResult> Save([FromBody] SapConfig dto) {
        if(User.FindFirst("role")?.Value!="admin") return Forbid();
        var c=await _db.SapConfigs.FirstOrDefaultAsync();
        if(c==null){_db.SapConfigs.Add(dto);}
        else{c.BaseUrl=dto.BaseUrl;c.ApiPath=dto.ApiPath;c.SapUsername=dto.SapUsername;if(!string.IsNullOrEmpty(dto.SapPassword))c.SapPassword=dto.SapPassword;c.ClientNo=dto.ClientNo;c.AccountGrp=dto.AccountGrp;c.Mode=dto.Mode;c.TimeoutMs=dto.TimeoutMs;c.UpdatedAt=DateTime.UtcNow;}
        await _db.SaveChangesAsync(); return Ok(new{message="SAP configuration saved."});
    }
}