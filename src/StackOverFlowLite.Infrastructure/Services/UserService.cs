using Microsoft.AspNetCore.Identity;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Infrastructure.Identity;

namespace StackOverflowLite.Infrastructure.Services;

/// <summary>
/// Implements IUserService using ASP.NET Identity's UserManager.
/// ApplicationUser is Infrastructure's own type — it never leaks to Application or Domain.
/// All user data is mapped to the plain UserResult record before crossing the boundary.
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserResult?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user == null ? null : ToResult(user);
    }

    public async Task<UserResult?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user == null ? null : ToResult(user);
    }

    public async Task<bool> CheckPasswordAsync(string userId, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<CreateUserResult> CreateAsync(
        string userName, string email, string password, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            Reputation = 0
        };

        var result = await _userManager.CreateAsync(user, password);
        return new CreateUserResult(
            result.Succeeded,
            result.Succeeded ? user.Id : null,
            result.Errors.Select(e => e.Description));
    }

    public async Task<bool> UpdateReputationAsync(
        string userId, int delta, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.Reputation = Math.Max(0, user.Reputation + delta);
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<int> GetReputationAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.Reputation ?? 0;
    }

    // Maps the Identity user to a plain record — the boundary crossing point
    private static UserResult ToResult(ApplicationUser user) =>
        new(user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty, user.Reputation);
}
