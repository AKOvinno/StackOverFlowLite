using FluentValidation;
using MediatR;
using StackOverflowLite.Application.Common.Exceptions;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Tags;

// Command
public record CreateTagCommand(string Name) : IRequest<TagDto>;

// Handler
public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly ITagRepository _tagRepository;

    public CreateTagCommandHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<TagDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.ToLower().Trim();

        var existing = await _tagRepository.FindByNameAsync(name, cancellationToken);
        if (existing != null)
            throw new BadRequestException($"Tag '{name}' already exists.");

        var tag = new Tag { Name = name };
        await _tagRepository.AddAsync(tag, cancellationToken);
        await _tagRepository.SaveChangesAsync(cancellationToken);

        return new TagDto(tag.Id, tag.Name);
    }
}

// Validator
public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required.")
            .MaximumLength(50).WithMessage("Tag name must not exceed 50 characters.");
    }
}
