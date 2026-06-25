using MediatR;
using StackOverflowLite.Application.Common.Exceptions;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Questions;

// Command
public record DeleteQuestionCommand(Guid Id) : IRequest;

// Handler
public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public DeleteQuestionCommandHandler(
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException();

        var question = await _questionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Question), request.Id);

        if (question.AuthorId != userId)
            throw new ForbiddenAccessException();

        _questionRepository.Remove(question);
        await _questionRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"question:{request.Id}", cancellationToken);
        await _cacheService.RemoveByPrefixAsync("questions:", cancellationToken);
    }
}
