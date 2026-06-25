using MediatR;
using StackOverflowLite.Application.Common.Interfaces;

namespace StackOverflowLite.Application.Features.Answers;

// Query
public record GetAnswersByQuestionQuery(Guid QuestionId) : IRequest<IEnumerable<AnswerDto>>;

// Handler — no EF, no Include; projection done in IAnswerRepository implementation
public class GetAnswersByQuestionQueryHandler : IRequestHandler<GetAnswersByQuestionQuery, IEnumerable<AnswerDto>>
{
    private readonly IAnswerRepository _answerRepository;

    public GetAnswersByQuestionQueryHandler(IAnswerRepository answerRepository)
    {
        _answerRepository = answerRepository;
    }

    public async Task<IEnumerable<AnswerDto>> Handle(GetAnswersByQuestionQuery request, CancellationToken cancellationToken)
    {
        var details = await _answerRepository.GetByQuestionIdAsync(request.QuestionId, cancellationToken);

        return details.Select(a => new AnswerDto(
            a.Id, a.Content, a.QuestionId, a.AuthorId,
            a.AuthorName, a.IsAccepted, a.VoteScore,
            a.CreatedAt, a.UpdatedAt));
    }
}
