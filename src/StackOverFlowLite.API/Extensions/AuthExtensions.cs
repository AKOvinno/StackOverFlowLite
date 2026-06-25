using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using StackOverflowLite.Infrastructure.Identity;
using StackOverflowLite.Infrastructure.Persistence.Context;

namespace StackOverflowLite.API.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddIdentityWithJwt(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ApplicationUser is from Infrastructure.Identity — not from Domain
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        var jwtSettings = configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

        services.AddAuthorization();
        return services;
    }
}
