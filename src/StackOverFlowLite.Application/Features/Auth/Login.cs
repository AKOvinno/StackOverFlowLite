using FluentValidation;
using MediatR;
using StackOverflowLite.Application.Common.Exceptions;
using StackOverflowLite.Application.Common.Interfaces;

namespace StackOverflowLite.Application.Features.Auth;

// Command
public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

// Handler — depends only on IUserService and IJwtService, no Identity types
public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.FindByEmailAsync(request.Email, cancellationToken)
            ?? throw new BadRequestException("Invalid email or password.");

        var passwordValid = await _userService.CheckPasswordAsync(user.Id, request.Password, cancellationToken);
        if (!passwordValid)
            throw new BadRequestException("Invalid email or password.");

        var token = _jwtService.GenerateToken(user.Id, user.UserName, user.Email);
        return new AuthResponseDto(token, user.Id, user.UserName, user.Email);
    }
}

// Validator
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
