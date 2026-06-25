using MediatR;
using StackOverflowLite.Application.Common.Interfaces;

namespace StackOverflowLite.Application.Features.Questions;

// Query
public record GetQuestionsQuery(
    string? Keyword = null,
    string? Tag = null,
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<QuestionSummaryDto>>;

// Handler — all filtering/paging/projection delegated to IQuestionRepository
public class GetQuestionsQueryHandler : IRequestHandler<GetQuestionsQuery, PagedResult<QuestionSummaryDto>>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICacheService _cacheService;

    public GetQuestionsQueryHandler(IQuestionRepository questionRepository, ICacheService cacheService)
    {
        _questionRepository = questionRepository;
        _cacheService = cacheService;
    }

    public async Task<PagedResult<QuestionSummaryDto>> Handle(GetQuestionsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"questions:page={request.Page}&size={request.PageSize}&kw={request.Keyword}&tag={request.Tag}";
        var cached = await _cacheService.GetAsync<PagedResult<QuestionSummaryDto>>(cacheKey, cancellationToken);
        if (cached != null) return cached;

        // Filtering, LINQ, EF paging all happen inside the repository — handler is clean
        var paged = await _questionRepository.GetPagedAsync(
            request.Keyword, request.Tag, request.Page, request.PageSize, cancellationToken);

        var items = paged.Items.Select(q => new QuestionSummaryDto(
            q.Id, q.Title, q.AuthorId, q.AuthorName,
            q.ViewCount, q.AnswerCount, q.VoteScore,
            q.HasAcceptedAnswer, q.Tags, q.CreatedAt));

        var result = new PagedResult<QuestionSummaryDto>(items, paged.TotalCount, request.Page, request.PageSize);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);
        return result;
    }
}
