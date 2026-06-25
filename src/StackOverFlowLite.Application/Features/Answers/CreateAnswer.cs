using FluentValidation;
using MediatR;
using StackOverflowLite.Application.Common.Exceptions;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Answers;

// Command
public record CreateAnswerCommand(Guid QuestionId, string Content) : IRequest<Guid>;

// Handler
public class CreateAnswerCommandHandler : IRequestHandler<CreateAnswerCommand, Guid>
{
    private readonly IAnswerRepository _answerRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateAnswerCommandHandler(
        IAnswerRepository answerRepository,
        IQuestionRepository questionRepository,
        ICurrentUserService currentUserService)
    {
        _answerRepository = answerRepository;
        _questionRepository = questionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateAnswerCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException();

        _ = await _questionRepository.GetByIdAsync(request.QuestionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Question), request.QuestionId);

        var answer = new Answer
        {
            Content = request.Content,
            QuestionId = request.QuestionId,
            AuthorId = userId
        };

        await _answerRepository.AddAsync(answer, cancellationToken);
        await _answerRepository.SaveChangesAsync(cancellationToken);

        return answer.Id;
    }
}

// Validator
public class CreateAnswerCommandValidator : AbstractValidator<CreateAnswerCommand>
{
    public CreateAnswerCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Answer content is required.");
    }
}
