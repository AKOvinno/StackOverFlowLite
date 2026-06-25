using Microsoft.AspNetCore.Identity;

namespace StackOverflowLite.Infrastructure.Identity;

/// <summary>
/// The Identity user class — lives in Infrastructure only.
/// Domain and Application never reference this type.
/// It maps to ASP.NET Identity tables (AspNetUsers, etc.).
/// The Reputation column is stored here so UserManager can manage it;
/// the value is surfaced to Application via IUserService.UserResult.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public int Reputation { get; set; } = 0;
}
