using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication.Data;

namespace WebApplication.Controllers;

public class LoginRequest { public string Email { get; set; } = null!; public string Password { get; set; } = null!; }

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SchoolContext _context;
    private readonly IConfiguration _config;

    public AuthController(SchoolContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var user = _context.Users.SingleOrDefault(u => u.Email == request.Email);

        if (user == null || user.PasswordHash != request.Password)
            return Unauthorized();

        var jwtSecret = _config["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JwtSettings:Secret missing");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { token = tokenString });
    }
}