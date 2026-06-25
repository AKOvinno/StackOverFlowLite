namespace StackOverflowLite.Application.Common.Interfaces;

/// <summary>
/// Application-level abstraction for user management.
/// No Identity types cross this boundary — handlers never see UserManager.
/// </summary>
public interface IUserService
{
    Task<UserResult?> FindByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserResult?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> CheckPasswordAsync(string userId, string password, CancellationToken cancellationToken = default);
    Task<CreateUserResult> CreateAsync(string userName, string email, string password, CancellationToken cancellationToken = default);
    Task<bool> UpdateReputationAsync(string userId, int delta, CancellationToken cancellationToken = default);
    Task<int> GetReputationAsync(string userId, CancellationToken cancellationToken = default);
}

/// <summary>Plain DTO returned by IUserService — no Identity types.</summary>
public record UserResult(string Id, string UserName, string Email, int Reputation);

/// <summary>Result of a create operation — carries errors without throwing.</summary>
public record CreateUserResult(bool Succeeded, string? UserId, IEnumerable<string> Errors);
