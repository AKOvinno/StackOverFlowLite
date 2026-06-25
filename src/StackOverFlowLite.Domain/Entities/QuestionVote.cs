using StackOverflowLite.Domain.Common;
using StackOverflowLite.Domain.Enums;

namespace StackOverflowLite.Domain.Entities;

/// <summary>
/// Pure domain entity. UserId is a FK → AspNetUsers.Id, configured by EF in Persistence.
/// </summary>
public class QuestionVote : BaseEntity
{
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;     // FK → AspNetUsers.Id
    public VoteType VoteType { get; set; }
}
