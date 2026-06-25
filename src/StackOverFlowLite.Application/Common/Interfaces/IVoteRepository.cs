using StackOverflowLite.Domain.Entities;
using StackOverflowLite.Domain.Enums;

namespace StackOverflowLite.Application.Common.Interfaces;

/// <summary>
/// Business-oriented vote data access contract.
/// Exposes semantic operations instead of raw EF queries.
/// </summary>
public interface IVoteRepository
{
    Task<QuestionVote?> GetQuestionVoteAsync(Guid questionId, string userId, CancellationToken cancellationToken = default);
    Task<AnswerVote?> GetAnswerVoteAsync(Guid answerId, string userId, CancellationToken cancellationToken = default);
    Task<int> GetQuestionVoteScoreAsync(Guid questionId, CancellationToken cancellationToken = default);
    Task<int> GetAnswerVoteScoreAsync(Guid answerId, CancellationToken cancellationToken = default);
    Task AddQuestionVoteAsync(QuestionVote vote, CancellationToken cancellationToken = default);
    Task AddAnswerVoteAsync(AnswerVote vote, CancellationToken cancellationToken = default);
    void UpdateQuestionVote(QuestionVote vote);
    void UpdateAnswerVote(AnswerVote vote);
    void RemoveQuestionVote(QuestionVote vote);
    void RemoveAnswerVote(AnswerVote vote);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
