using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackOverflowLite.Domain.Entities;
using StackOverflowLite.Infrastructure.Identity;

namespace StackOverflowLite.Infrastructure.Persistence.Configurations;

public class QuestionVoteConfiguration : IEntityTypeConfiguration<QuestionVote>
{
    public void Configure(EntityTypeBuilder<QuestionVote> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.VoteType)
            .IsRequired();

        builder.Property(v => v.UserId)
            .IsRequired();

        // FK to AspNetUsers — no navigation property on the Domain entity.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => new { v.QuestionId, v.UserId })
            .IsUnique();
    }
}
