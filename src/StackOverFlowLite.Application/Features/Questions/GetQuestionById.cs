using MediatR;
using StackOverflowLite.Application.Common.Exceptions;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Questions;

// Query
public record GetQuestionByIdQuery(Guid Id, string? UserIdOrIp = null) : IRequest<QuestionDetailDto>;

// Handler — calls IQuestionRepository.GetByIdWithDetailsAsync(); no EF in this layer
public class GetQuestionByIdQueryHandler : IRequestHandler<GetQuestionByIdQuery, QuestionDetailDto>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICacheService _cacheService;

    public GetQuestionByIdQueryHandler(IQuestionRepository questionRepository, ICacheService cacheService)
    {
        _questionRepository = questionRepository;
        _cacheService = cacheService;
    }

    public async Task<QuestionDetailDto> Handle(GetQuestionByIdQuery request, CancellationToken cancellationToken)
    {
        // Track view before fetching — once per user/IP per 30 min (Redis-backed in ICacheService)
        await TrackViewAsync(request.Id, request.UserIdOrIp, cancellationToken);

        var cacheKey = $"question:{request.Id}";
        var cached = await _cacheService.GetAsync<QuestionDetailDto>(cacheKey, cancellationToken);
        if (cached != null) return cached;

        // All projection (Include, joins, vote sum) happens inside the repository implementation
        var detail = await _questionRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Question), request.Id);

        var dto = new QuestionDetailDto(
            detail.Id, detail.Title, detail.Description, detail.AuthorId, detail.AuthorName,
            detail.ViewCount, detail.AcceptedAnswerId, detail.VoteScore,
            detail.Tags, detail.CreatedAt, detail.UpdatedAt);

        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), cancellationToken);
        return dto;
    }

    private async Task TrackViewAsync(Guid questionId, string? userIdOrIp, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userIdOrIp)) return;

        var viewKey = $"view:{questionId}:{userIdOrIp}";
        if (!await _cacheService.ExistsAsync(viewKey, cancellationToken))
        {
            await _questionRepository.IncrementViewCountAsync(questionId, cancellationToken);
            await _cacheService.SetAsync(viewKey, true, TimeSpan.FromMinutes(30), cancellationToken);
            // Invalidate the cached detail so the new ViewCount is reflected
            await _cacheService.RemoveAsync($"question:{questionId}", cancellationToken);
        }
    }
}
