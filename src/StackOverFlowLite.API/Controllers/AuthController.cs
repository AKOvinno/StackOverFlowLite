using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverflowLite.Application.Features.Auth;

namespace StackOverflowLite.API.Controllers;

// Custom route kept — overrides BaseApiController's [Route("api/[controller]")]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    /// <summary>Register a new user account.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await Mediator.Send(new RegisterCommand(dto.UserName, dto.Email, dto.Password));
        return CreatedAtAction(nameof(GetMe), result);
    }

    /// <summary>Login and receive a JWT token.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await Mediator.Send(new LoginCommand(dto.Email, dto.Password));
        return Ok(result);
    }

    /// <summary>Get the current authenticated user's profile including reputation.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var result = await Mediator.Send(new GetMeQuery());
        return Ok(result);
    }
}
