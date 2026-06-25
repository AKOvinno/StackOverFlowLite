using MediatR;
using StackOverflowLite.Application.Common.Exceptions;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Answers;

// Command
public record AcceptAnswerCommand(Guid QuestionId, Guid AnswerId) : IRequest;

// Handler — reputation updated through IUserService; no UserManager reference
public class AcceptAnswerCommandHandler : IRequestHandler<AcceptAnswerCommand>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerRepository _answerRepository;
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public AcceptAnswerCommandHandler(
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

    public async Task Handle(AcceptAnswerCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException();

        var question = await _questionRepository.GetByIdAsync(request.QuestionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Question), request.QuestionId);

        if (question.AuthorId != userId)
            throw new ForbiddenAccessException();

        var answer = await _answerRepository.GetByIdAsync(request.AnswerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Answer), request.AnswerId);

        if (answer.QuestionId != request.QuestionId)
            throw new BadRequestException("This answer does not belong to the specified question.");

        if (answer.AuthorId == userId)
            throw new BadRequestException("You cannot accept your own answer.");

        // Remove +15 from previous accepted answer's author if switching
        if (question.AcceptedAnswerId.HasValue && question.AcceptedAnswerId != request.AnswerId)
        {
            var oldAnswer = await _answerRepository.GetByIdAsync(question.AcceptedAnswerId.Value, cancellationToken);
            if (oldAnswer != null)
            {
                oldAnswer.IsAccepted = false;
                _answerRepository.Update(oldAnswer);
                await _userService.UpdateReputationAsync(oldAnswer.AuthorId, -15, cancellationToken);
            }
        }

        // Grant +15 to new accepted answer's author (only if not already accepted)
        if (question.AcceptedAnswerId != request.AnswerId)
            await _userService.UpdateReputationAsync(answer.AuthorId, +15, cancellationToken);

        question.AcceptedAnswerId = request.AnswerId;
        question.UpdatedAt = DateTime.UtcNow;
        answer.IsAccepted = true;

        _questionRepository.Update(question);
        _answerRepository.Update(answer);
        await _questionRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"question:{request.QuestionId}", cancellationToken);
        await _cacheService.RemoveByPrefixAsync("questions:", cancellationToken);
    }
}
