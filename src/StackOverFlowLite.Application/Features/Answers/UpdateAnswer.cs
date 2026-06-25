using FluentValidation;
using MediatR;
using StackOverflowLite.Application.Common.Exceptions;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Answers;

// Command
public record UpdateAnswerCommand(Guid Id, string Content) : IRequest;

// Handler
public class UpdateAnswerCommandHandler : IRequestHandler<UpdateAnswerCommand>
{
    private readonly IAnswerRepository _answerRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateAnswerCommandHandler(IAnswerRepository answerRepository, ICurrentUserService currentUserService)
    {
        _answerRepository = answerRepository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateAnswerCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException();

        var answer = await _answerRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Answer), request.Id);

        if (answer.AuthorId != userId)
            throw new ForbiddenAccessException();

        answer.Content = request.Content;
        answer.UpdatedAt = DateTime.UtcNow;

        _answerRepository.Update(answer);
        await _answerRepository.SaveChangesAsync(cancellationToken);
    }
}

// Validator
public class UpdateAnswerCommandValidator : AbstractValidator<UpdateAnswerCommand>
{
    public UpdateAnswerCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Answer content is required.");
    }
}
