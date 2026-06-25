namespace StackOverflowLite.Application.Features.Answers;

public record AnswerDto(
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
