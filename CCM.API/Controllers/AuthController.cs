using CCM.API.DTOs; using CCM.API.Services; using Microsoft.AspNetCore.Mvc;
namespace CCM.API.Controllers;
[ApiController][Route("api/[controller]")]
public class AuthController : ControllerBase {
    private readonly AuthService _auth;
    public AuthController(AuthService a){_auth=a;}
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto) {
        var(ok,token,user)=await _auth.LoginAsync(dto.Username,dto.Password);
        if(!ok) return Unauthorized(new{error="Invalid username or password"});
        return Ok(new{token,user=new{user!.Id,user.Username,user.FullName,user.Email,user.Role}});
    }
}