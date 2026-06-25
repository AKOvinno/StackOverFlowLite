using StackOverflowLite.Domain.Entities;
using StackOverflowLite.Domain.Enums;

namespace StackOverflowLite.Application.Common.Interfaces;

/// <summary>
/// Business-oriented question data access contract.
/// No IQueryable, no Include(), no EF types cross this boundary.
/// </summary>
public interface IQuestionRepository
{
    Task<QuestionDetail?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedQuestions> GetPagedAsync(string? keyword, string? tag, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Question?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Question?> GetByIdWithTagsAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Question question, CancellationToken cancellationToken = default);
    void Update(Question question);
    void Remove(Question question);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task IncrementViewCountAsync(Guid questionId, CancellationToken cancellationToken = default);
}

/// <summary>Rich read model returned for GET /questions/{id} — fully projected in Infrastructure.</summary>
public record QuestionDetail(
    Guid Id,
    string Title,
    string Description,
    string AuthorId,
    string AuthorName,
    int ViewCount,
    Guid? AcceptedAnswerId,
    int VoteScore,
    IEnumerable<string> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

/// <summary>Paged summary list returned for GET /questions.</summary>
public record PagedQuestions(
    IEnumerable<QuestionSummary> Items,
    int TotalCount
);

public record QuestionSummary(
    Guid Id,
    string Title,
    string AuthorId,
    string AuthorName,
    int ViewCount,
    int AnswerCount,
    int VoteScore,
    bool HasAcceptedAnswer,
    IEnumerable<string> Tags,
    DateTime CreatedAt
);
