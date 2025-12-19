using Microsoft.AspNetCore.Mvc;
using Northwind.WebApi.Services;

namespace Northwind.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(IJwtService jwtService) : ControllerBase
{
    // Demo users (in production, use proper identity store)
    private static readonly Dictionary<string, (string Password, string[] Roles)> Users = new()
    {
        ["admin@northwind.com"] = ("admin123", new[] { "Admin", "User" }),
        ["user@northwind.com"] = ("user123", new[] { "User" })
    };

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!Users.TryGetValue(request.Email, out var user) || user.Password != request.Password)
            return Unauthorized(new { message = "Invalid credentials" });

        var token = jwtService.GenerateToken(request.Email, request.Email, user.Roles);
        return Ok(new LoginResponse(token, DateTime.UtcNow.AddMinutes(60), user.Roles));
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), 201)]
    [ProducesResponseType(400)]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (Users.ContainsKey(request.Email))
            return BadRequest(new { message = "User already exists" });

        // In demo, just return success (don't actually store)
        var token = jwtService.GenerateToken(request.Email, request.Email, new[] { "User" });
        return Created("", new RegisterResponse(request.Email, token));
    }
}

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, DateTime ExpiresAt, string[] Roles);
public record RegisterRequest(string Email, string Password, string? Name);
public record RegisterResponse(string Email, string Token);
