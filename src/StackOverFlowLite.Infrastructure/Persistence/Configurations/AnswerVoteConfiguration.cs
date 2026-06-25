using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackOverflowLite.Domain.Entities;
using StackOverflowLite.Infrastructure.Identity;

namespace StackOverflowLite.Infrastructure.Persistence.Configurations;

public class AnswerVoteConfiguration : IEntityTypeConfiguration<AnswerVote>
{
    public void Configure(EntityTypeBuilder<AnswerVote> builder)
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

        builder.HasIndex(v => new { v.AnswerId, v.UserId })
            .IsUnique();
    }
}
