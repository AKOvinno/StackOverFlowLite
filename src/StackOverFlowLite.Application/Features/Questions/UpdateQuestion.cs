using FluentValidation;
using MediatR;
using StackOverflowLite.Application.Common.Exceptions;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Questions;

// Command
public record UpdateQuestionCommand(Guid Id, string Title, string Description, IEnumerable<string> Tags) : IRequest;

// Handler
public class UpdateQuestionCommandHandler : IRequestHandler<UpdateQuestionCommand>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public UpdateQuestionCommandHandler(
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

    public async Task Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException();

        // GetByIdWithTagsAsync returns the entity with QuestionTags loaded — EF details hidden in repo
        var question = await _questionRepository.GetByIdWithTagsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Question), request.Id);

        if (question.AuthorId != userId)
            throw new ForbiddenAccessException();

        question.Title = request.Title;
        question.Description = request.Description;
        question.UpdatedAt = DateTime.UtcNow;
        question.QuestionTags.Clear();

        foreach (var tagName in request.Tags.Select(t => t.ToLower().Trim()).Distinct())
        {
            var tag = await _tagRepository.FindByNameAsync(tagName, cancellationToken);
            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                await _tagRepository.AddAsync(tag, cancellationToken);
                await _tagRepository.SaveChangesAsync(cancellationToken);
            }
            question.QuestionTags.Add(new QuestionTag { QuestionId = question.Id, TagId = tag.Id });
        }

        _questionRepository.Update(question);
        await _questionRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"question:{request.Id}", cancellationToken);
        await _cacheService.RemoveByPrefixAsync("questions:", cancellationToken);
    }
}

// Validator
public class UpdateQuestionCommandValidator : AbstractValidator<UpdateQuestionCommand>
{
    public UpdateQuestionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");
    }
}
