using StackOverflowLite.Domain.Common;

namespace StackOverflowLite.Domain.Entities;

/// <summary>
/// Pure domain entity. No Identity, no EF attributes, no framework dependencies.
/// AuthorId is a FK string that maps to AspNetUsers.Id — wired by EF config in Persistence.
/// </summary>
public class Question : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;   // FK → AspNetUsers.Id
    public int ViewCount { get; set; } = 0;
    public Guid? AcceptedAnswerId { get; set; }
    public Answer? AcceptedAnswer { get; set; }
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
    public ICollection<QuestionVote> Votes { get; set; } = new List<QuestionVote>();
}
