using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StackOverflowLite.Domain.Entities;
using StackOverflowLite.Infrastructure.Identity;

namespace StackOverflowLite.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(q => q.Description)
            .IsRequired();

        builder.Property(q => q.AuthorId)
            .IsRequired();

        // FK to AspNetUsers — no navigation property on the Domain entity.
        // WithMany() uses the collection on ApplicationUser (Infrastructure), not on Question.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(q => q.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.AcceptedAnswer)
            .WithMany()
            .HasForeignKey(q => q.AcceptedAnswerId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(q => q.Votes)
            .WithOne(v => v.Question)
            .HasForeignKey(v => v.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(q => q.AuthorId);
        builder.HasIndex(q => q.CreatedAt);
    }
}
