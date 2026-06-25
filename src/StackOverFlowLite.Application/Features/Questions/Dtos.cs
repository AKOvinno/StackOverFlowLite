namespace StackOverflowLite.Application.Features.Questions;

public record QuestionSummaryDto(
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

public record QuestionDetailDto(
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

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);
