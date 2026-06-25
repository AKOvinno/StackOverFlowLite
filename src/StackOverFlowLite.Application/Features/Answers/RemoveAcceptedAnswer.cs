using MediatR;
using StackOverflowLite.Application.Common.Exceptions;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Answers;

// Command
public record RemoveAcceptedAnswerCommand(Guid QuestionId) : IRequest;

// Handler
public class RemoveAcceptedAnswerCommandHandler : IRequestHandler<RemoveAcceptedAnswerCommand>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerRepository _answerRepository;
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public RemoveAcceptedAnswerCommandHandler(
        IQuestionRepository questionRepository,
        IAnswerRepository answerRepository,
        IUserService userService,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _questionRepository = questionRepository;
        _answerRepository = answerRepository;
        _userService = userService;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task Handle(RemoveAcceptedAnswerCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException();

        var question = await _questionRepository.GetByIdAsync(request.QuestionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Question), request.QuestionId);

        if (question.AuthorId != userId)
            throw new ForbiddenAccessException();

        if (!question.AcceptedAnswerId.HasValue)
            throw new BadRequestException("This question has no accepted answer.");

        var acceptedAnswer = await _answerRepository.GetByIdAsync(question.AcceptedAnswerId.Value, cancellationToken);
        if (acceptedAnswer != null)
        {
            acceptedAnswer.IsAccepted = false;
            _answerRepository.Update(acceptedAnswer);
            // Remove the +15 bonus through IUserService — no UserManager reference
            await _userService.UpdateReputationAsync(acceptedAnswer.AuthorId, -15, cancellationToken);
        }

        question.AcceptedAnswerId = null;
        question.UpdatedAt = DateTime.UtcNow;
        _questionRepository.Update(question);
        await _questionRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"question:{request.QuestionId}", cancellationToken);
        await _cacheService.RemoveByPrefixAsync("questions:", cancellationToken);
    }
}
