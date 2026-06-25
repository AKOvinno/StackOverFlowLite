using StackOverflowLite.Domain.Common;

namespace StackOverflowLite.Domain.Entities;

/// <summary>
/// Pure domain entity. AuthorId is a FK → AspNetUsers.Id, configured by EF in Persistence.
/// </summary>
public class Answer : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public string AuthorId { get; set; } = string.Empty;   // FK → AspNetUsers.Id
    public bool IsAccepted { get; set; } = false;
    public ICollection<AnswerVote> Votes { get; set; } = new List<AnswerVote>();
}
