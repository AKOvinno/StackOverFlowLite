using StackOverflowLite.Domain.Common;
using StackOverflowLite.Domain.Enums;

namespace StackOverflowLite.Domain.Entities;

/// <summary>
/// Pure domain entity. UserId is a FK → AspNetUsers.Id, configured by EF in Persistence.
/// </summary>
public class AnswerVote : BaseEntity
{
    public Guid AnswerId { get; set; }
    public Answer Answer { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;     // FK → AspNetUsers.Id
    public VoteType VoteType { get; set; }
}
