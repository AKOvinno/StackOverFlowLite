using MediatR;
using StackOverflowLite.Application.Common.Interfaces;

namespace StackOverflowLite.Application.Features.Tags;

// Query
public record GetTagsQuery : IRequest<IEnumerable<TagDto>>;

// Handler
public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, IEnumerable<TagDto>>
{
    private readonly ITagRepository _tagRepository;

    public GetTagsQueryHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<IEnumerable<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _tagRepository.GetAllAsync(cancellationToken);
        return tags.OrderBy(t => t.Name).Select(t => new TagDto(t.Id, t.Name));
    }
}
