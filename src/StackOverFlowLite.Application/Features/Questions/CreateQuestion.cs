using FluentValidation;
using MediatR;
using StackOverflowLite.Application.Common.Exceptions;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Questions;

// Command
public record CreateQuestionCommand(string Title, string Description, IEnumerable<string> Tags) : IRequest<Guid>;

// Handler — uses IQuestionRepository and ITagRepository only, no EF types
public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, Guid>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public CreateQuestionCommandHandler(
        IQuestionRepository questionRepository,
        ITagRepository tagRepository,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _questionRepository = questionRepository;
        _tagRepository = tagRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<Guid> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException();

        var question = new Question
        {
            Title = request.Title,
            Description = request.Description,
            AuthorId = userId
        };

        foreach (var tagName in request.Tags.Select(t => t.ToLower().Trim()).Distinct())
        {
            var tag = await _tagRepository.FindByNameAsync(tagName, cancellationToken);
            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                await _tagRepository.AddAsync(tag, cancellationToken);
                await _tagRepository.SaveChangesAsync(cancellationToken);
            }
            question.QuestionTags.Add(new QuestionTag { TagId = tag.Id });
        }

        await _questionRepository.AddAsync(question, cancellationToken);
        await _questionRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync("questions:", cancellationToken);

        return question.Id;
    }
}

// Validator
public class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");
    }
}
