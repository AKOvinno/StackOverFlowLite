using MediatR;
using StackOverflowLite.Application.Common.Exceptions;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Answers;

// Command
public record DeleteAnswerCommand(Guid Id) : IRequest;

// Handler
public class DeleteAnswerCommandHandler : IRequestHandler<DeleteAnswerCommand>
{
    private readonly IAnswerRepository _answerRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteAnswerCommandHandler(
        IAnswerRepository answerRepository,
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService)
    {
        _answerRepository = answerRepository;
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteAnswerCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException();

        var answer = await _answerRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Answer), request.Id);

        if (answer.AuthorId != userId)
            throw new ForbiddenAccessException();

        var question = await _questionRepository.GetByIdAsync(answer.QuestionId, cancellationToken);
        if (question?.AcceptedAnswerId == answer.Id)
            throw new BadRequestException("Cannot delete an accepted answer. Remove acceptance first.");

        _answerRepository.Remove(answer);
        await _answerRepository.SaveChangesAsync(cancellationToken);
    }
}
