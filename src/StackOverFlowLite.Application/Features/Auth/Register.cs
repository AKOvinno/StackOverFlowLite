using FluentValidation;
using MediatR;
using StackOverflowLite.Application.Common.Exceptions;
using StackOverflowLite.Application.Common.Interfaces;
using ValidationException = FluentValidation.ValidationException;

namespace StackOverflowLite.Application.Features.Auth;

// Command
public record RegisterCommand(string UserName, string Email, string Password) : IRequest<AuthResponseDto>;

// Handler — depends only on IUserService and IJwtService, no Identity types
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userService.FindByEmailAsync(request.Email, cancellationToken);
        if (existing != null)
            throw new BadRequestException("Email is already registered.");

        var result = await _userService.CreateAsync(request.UserName, request.Email, request.Password, cancellationToken);
        if (!result.Succeeded)
        {
            var failures = result.Errors.Select(e =>
                new FluentValidation.Results.ValidationFailure("Identity", e));
            throw new ValidationException(failures);
        }

        var user = await _userService.FindByIdAsync(result.UserId!, cancellationToken)
            ?? throw new BadRequestException("User creation failed.");

        var token = _jwtService.GenerateToken(user.Id, user.UserName, user.Email);
        return new AuthResponseDto(token, user.Id, user.UserName, user.Email);
    }
}

// Validator
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}
