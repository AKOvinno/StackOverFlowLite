using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverflowLite.Application.Features.Votes;
using StackOverflowLite.Domain.Enums;

namespace StackOverflowLite.API.Controllers;

[Route("api")]
[Authorize]
public class VotesController : BaseApiController
{
    /// <summary>Vote on a question. VoteType: 1 = Upvote, -1 = Downvote. Toggling the same vote removes it.</summary>
    [HttpPost("questions/{id:guid}/vote")]
    public async Task<IActionResult> VoteQuestion(Guid id, [FromBody] VoteRequest request)
    {
        var result = await Mediator.Send(new VoteQuestionCommand(id, request.VoteType));
        return Ok(result);
    }

    /// <summary>Vote on an answer. VoteType: 1 = Upvote, -1 = Downvote. Toggling the same vote removes it.</summary>
    [HttpPost("answers/{id:guid}/vote")]
    public async Task<IActionResult> VoteAnswer(Guid id, [FromBody] VoteRequest request)
    {
        var result = await Mediator.Send(new VoteAnswerCommand(id, request.VoteType));
        return Ok(result);
    }
}

public record VoteRequest(VoteType VoteType);
