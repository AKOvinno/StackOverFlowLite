using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverflowLite.Application.Features.Answers;

namespace StackOverflowLite.API.Controllers;

// Non-standard route kept — answers are nested under /api/questions/{id}/answers
[Route("api")]
public class AnswersController : BaseApiController
{
    /// <summary>Get all answers for a specific question.</summary>
    [HttpGet("questions/{questionId:guid}/answers")]
    public async Task<IActionResult> GetAnswers(Guid questionId)
    {
        var result = await Mediator.Send(new GetAnswersByQuestionQuery(questionId));
        return Ok(result);
    }

    /// <summary>Post an answer to a question.</summary>
    [HttpPost("questions/{questionId:guid}/answers")]
    [Authorize]
    public async Task<IActionResult> CreateAnswer(Guid questionId, [FromBody] CreateAnswerRequest request)
    {
        var id = await Mediator.Send(new CreateAnswerCommand(questionId, request.Content));
        return Created($"/api/questions/{questionId}/answers", new { id });
    }

    /// <summary>Update an answer (author only).</summary>
    [HttpPut("answers/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateAnswer(Guid id, [FromBody] UpdateAnswerRequest request)
    {
        await Mediator.Send(new UpdateAnswerCommand(id, request.Content));
        return NoContent();
    }

    /// <summary>Delete an answer (author only; accepted answers cannot be deleted).</summary>
    [HttpDelete("answers/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteAnswer(Guid id)
    {
        await Mediator.Send(new DeleteAnswerCommand(id));
        return NoContent();
    }

    /// <summary>Accept an answer as the best solution (question author only).</summary>
    [HttpPost("questions/{questionId:guid}/accept/{answerId:guid}")]
    [Authorize]
    public async Task<IActionResult> AcceptAnswer(Guid questionId, Guid answerId)
    {
        await Mediator.Send(new AcceptAnswerCommand(questionId, answerId));
        return NoContent();
    }

    /// <summary>Remove the accepted answer from a question (question author only).</summary>
    [HttpDelete("questions/{questionId:guid}/accept")]
    [Authorize]
    public async Task<IActionResult> RemoveAcceptedAnswer(Guid questionId)
    {
        await Mediator.Send(new RemoveAcceptedAnswerCommand(questionId));
        return NoContent();
    }
}

public record CreateAnswerRequest(string Content);
public record UpdateAnswerRequest(string Content);
