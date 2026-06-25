using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StackOverflowLite.API.Controllers;

/// <summary>
/// Shared base for all API controllers.
/// Provides:
///   - a lazy <see cref="Mediator"/> property so subclasses never need
///     a constructor parameter or private field for IMediator.
///   - <see cref="ToActionResult{T}"/> overloads that convert an
///     <see cref="ErrorOr{T}"/> result into the correct HTTP response,
///     keeping error-mapping logic in one place.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    // Resolved lazily from the DI container so subclasses need no constructor.
    protected IMediator Mediator =>
        HttpContext.RequestServices.GetRequiredService<IMediator>();

    // ------------------------------------------------------------------ //
    //  ToActionResult overloads — one per "success shape"                 //
    // ------------------------------------------------------------------ //

    /// <summary>Maps ErrorOr&lt;T&gt; → 200 OK with value, or a Problem response.</summary>
    protected ActionResult<T> ToActionResult<T>(ErrorOr<T> result) =>
        result.Match(
            value  => Ok(value),
            errors => Problem(errors));

    /// <summary>Maps ErrorOr&lt;Success&gt; → 200 OK (no body), or a Problem response.</summary>
    protected ActionResult ToActionResult(ErrorOr<Success> result) =>
        result.Match(
            _      => (ActionResult)Ok(),
            errors => Problem(errors));

    /// <summary>Maps ErrorOr&lt;Deleted&gt; → 204 No Content, or a Problem response.</summary>
    protected ActionResult ToActionResult(ErrorOr<Deleted> result) =>
        result.Match(
            _      => (ActionResult)NoContent(),
            errors => Problem(errors));

    /// <summary>Maps ErrorOr&lt;Created&gt; → 201 Created (no body), or a Problem response.</summary>
    protected ActionResult ToActionResult(ErrorOr<Created> result) =>
        result.Match(
            _      => (ActionResult)StatusCode(StatusCodes.Status201Created),
            errors => Problem(errors));

    // ------------------------------------------------------------------ //
    //  Private error mapper                                                //
    // ------------------------------------------------------------------ //

    private ActionResult Problem(List<Error> errors)
    {
        var first = errors.First();

        var statusCode = first.Type switch
        {
            ErrorType.NotFound     => StatusCodes.Status404NotFound,
            ErrorType.Validation   => StatusCodes.Status400BadRequest,
            ErrorType.Conflict     => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden    => StatusCodes.Status403Forbidden,
            _                      => StatusCodes.Status500InternalServerError
        };

        return Problem(
            statusCode: statusCode,
            title:      first.Code,
            detail:     first.Description);
    }
}
