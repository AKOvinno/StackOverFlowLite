using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StackOverflowLite.Domain.Entities;
using StackOverflowLite.Infrastructure.Identity;

namespace StackOverflowLite.Infrastructure.Persistence.Context;

/// <summary>
/// DbContext lives in Infrastructure so that both ApplicationUser (Identity) and
/// Domain entities can be configured here without any circular project references.
/// 
/// Dependency graph is now strictly one-directional:
///   Domain ← Application ← Infrastructure ← Persistence (migrations only)
///                                          ← API
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Domain entity tables
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<QuestionTag> QuestionTags => Set<QuestionTag>();
    public DbSet<QuestionVote> QuestionVotes => Set<QuestionVote>();
    public DbSet<AnswerVote> AnswerVotes => Set<AnswerVote>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // All configurations live in this assembly (Infrastructure)
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
