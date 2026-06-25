namespace StackOverflowLite.Application.Features.Auth;

public record RegisterDto(string UserName, string Email, string Password);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, string UserId, string UserName, string Email);
public record UserProfileDto(string UserId, string UserName, string Email, int Reputation);
