using CCM.API.Models; using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt; using System.Security.Claims; using System.Text;
namespace CCM.API.Helpers;
public static class JwtHelper {
    public static string GenerateToken(User u, IConfiguration c) {
        var claims = new[]{ new Claim("id",u.Id.ToString()), new Claim("username",u.Username), new Claim("name",u.FullName), new Claim("email",u.Email), new Claim("role",u.Role) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(c["Jwt:Key"]!));
        var token = new JwtSecurityToken(c["Jwt:Issuer"],c["Jwt:Audience"],claims,expires:DateTime.UtcNow.AddMinutes(int.Parse(c["Jwt:ExpiryMinutes"]!)),signingCredentials:new SigningCredentials(key,SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}