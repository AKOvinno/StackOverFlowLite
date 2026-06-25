using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Common.Interfaces;

/// <summary>
/// Business-oriented answer data access contract.
/// No EF types, no IQueryable.
/// </summary>
public interface IAnswerRepository
{
    Task<Answer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AnswerDetail>> GetByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default);
    Task AddAsync(Answer answer, CancellationToken cancellationToken = default);
    void Update(Answer answer);
    void Remove(Answer answer);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>Rich read model for answer list — projected in Infrastructure.</summary>
public record AnswerDetail(
    Guid Id,
    string Content,
    Guid QuestionId,
    string AuthorId,
    string AuthorName,
    bool IsAccepted,
    int VoteScore,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
