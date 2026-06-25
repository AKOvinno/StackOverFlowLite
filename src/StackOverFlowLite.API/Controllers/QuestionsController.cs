using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverflowLite.Application.Features.Questions;

namespace StackOverflowLite.API.Controllers;

[Route("api/questions")]
public class QuestionsController : BaseApiController
{
    /// <summary>Get a paginated list of questions with optional search and tag filter.</summary>
    [HttpGet]
    public async Task<IActionResult> GetQuestions(
        [FromQuery] string? keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await Mediator.Send(new GetQuestionsQuery(keyword, null, page, pageSize));
        return Ok(result);
    }

    /// <summary>Get a single question by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetQuestionById(Guid id)
    {
        var userIdOrIp = User.Identity?.IsAuthenticated == true
            ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            : HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await Mediator.Send(new GetQuestionByIdQuery(id, userIdOrIp));
        return Ok(result);
    }

    /// <summary>Get questions filtered by tag name.</summary>
    [HttpGet("tag/{tag}")]
    public async Task<IActionResult> GetQuestionsByTag(
        string tag,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await Mediator.Send(new GetQuestionsQuery(null, tag, page, pageSize));
        return Ok(result);
    }

    /// <summary>Search questions by keyword.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchQuestions(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await Mediator.Send(new GetQuestionsQuery(keyword, null, page, pageSize));
        return Ok(result);
    }

    /// <summary>Create a new question.</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequest request)
    {
        var id = await Mediator.Send(new CreateQuestionCommand(request.Title, request.Description, request.Tags ?? []));
        return CreatedAtAction(nameof(GetQuestionById), new { id }, new { id });
    }

    /// <summary>Update an existing question (author only).</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] UpdateQuestionRequest request)
    {
        await Mediator.Send(new UpdateQuestionCommand(id, request.Title, request.Description, request.Tags ?? []));
        return NoContent();
    }

    /// <summary>Delete a question (author only).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteQuestion(Guid id)
    {
        await Mediator.Send(new DeleteQuestionCommand(id));
        return NoContent();
    }
}

public record CreateQuestionRequest(string Title, string Description, IEnumerable<string>? Tags);
public record UpdateQuestionRequest(string Title, string Description, IEnumerable<string>? Tags);
