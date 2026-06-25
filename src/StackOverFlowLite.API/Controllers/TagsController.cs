using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverflowLite.Application.Features.Tags;

namespace StackOverflowLite.API.Controllers;

[Route("api/tags")]
public class TagsController : BaseApiController
{
    /// <summary>Get all available tags.</summary>
    [HttpGet]
    public async Task<IActionResult> GetTags()
    {
        var result = await Mediator.Send(new GetTagsQuery());
        return Ok(result);
    }

    /// <summary>Create a new tag (authenticated users only).</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
    {
        var result = await Mediator.Send(new CreateTagCommand(request.Name));
        return CreatedAtAction(nameof(GetTags), result);
    }
}

public record CreateTagRequest(string Name);
