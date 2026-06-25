using MediatR;
using StackOverflowLite.Application.Common.Exceptions;
using StackOverflowLite.Application.Common.Interfaces;

namespace StackOverflowLite.Application.Features.Auth;

// Query
public record GetMeQuery : IRequest<UserProfileDto>;

// Handler — depends only on IUserService and ICurrentUserService
public class GetMeQueryHandler : IRequestHandler<GetMeQuery, UserProfileDto>
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public GetMeQueryHandler(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    public async Task<UserProfileDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new ForbiddenAccessException();

        var user = await _userService.FindByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        return new UserProfileDto(user.Id, user.UserName, user.Email, user.Reputation);
    }
}
