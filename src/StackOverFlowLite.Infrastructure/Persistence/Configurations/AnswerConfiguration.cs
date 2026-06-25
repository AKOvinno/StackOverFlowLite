using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackOverflowLite.Domain.Entities;
using StackOverflowLite.Infrastructure.Identity;

namespace StackOverflowLite.Infrastructure.Persistence.Configurations;

public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Content)
            .IsRequired();

        builder.Property(a => a.AuthorId)
            .IsRequired();

        // FK to AspNetUsers — no navigation property on the Domain entity.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Votes)
            .WithOne(v => v.Answer)
            .HasForeignKey(v => v.AnswerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.QuestionId);
        builder.HasIndex(a => a.AuthorId);
    }
}
