using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Infrastructure.Caching;
using StackOverflowLite.Infrastructure.Persistence.Context;
using StackOverflowLite.Infrastructure.Repositories;
using StackOverflowLite.Infrastructure.Services;

namespace StackOverflowLite.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext — moved here from Persistence project
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Redis caching
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));
        services.AddScoped<ICacheService, RedisCacheService>();

        // JWT generation
        services.AddScoped<IJwtService, JwtService>();

        // HttpContext accessor for current user resolution
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // IUserService wraps UserManager
        services.AddScoped<IUserService, UserService>();

        // Repository implementations
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IAnswerRepository, AnswerRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IVoteRepository, VoteRepository>();

        return services;
    }
}