namespace StackOverflowLite.Application.Common.Interfaces;

/// <summary>
/// JWT generation contract. Accepts plain values only — no Identity types.
/// </summary>
public interface IJwtService
{
    string GenerateToken(string userId, string userName, string email);
}
