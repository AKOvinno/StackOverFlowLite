using FluentValidation;
using MediatR;
using StackOverflowLite.Application.Common.Exceptions;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;
using StackOverflowLite.Domain.Enums;

namespace StackOverflowLite.Application.Features.Votes;

// Command
public record VoteAnswerCommand(Guid AnswerId, VoteType VoteType) : IRequest<VoteResultDto>;

// Handler
public class VoteAnswerCommandHandler : IRequestHandler<VoteAnswerCommand, VoteResultDto>
{
    private readonly IAnswerRepository _answerRepository;
    private readonly IVoteRepository _voteRepository;
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public VoteAnswerCommandHandler(
        IAnswerRepository answerRepository,
        IVoteRepository voteRepository,
        IUserService userService,
        ICurrentUserService currentUserService)
    {
        _answerRepository = answerRepository;
        _voteRepository = voteRepository;
        _userService = userService;
        _currentUserService = currentUserService;
    }

    public async Task<VoteResultDto> Handle(VoteAnswerCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException();

        var answer = await _answerRepository.GetByIdAsync(request.AnswerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Answer), request.AnswerId);

        if (answer.AuthorId == userId)
            throw new BadRequestException("You cannot vote on your own answer.");

        var existingVote = await _voteRepository.GetAnswerVoteAsync(
            request.AnswerId, userId, cancellationToken);

        if (existingVote != null)
        {
            // Reverse old reputation: upvote gave +10, downvote gave -2
            var oldDelta = existingVote.VoteType == VoteType.Upvote ? 10 : -2;
            await _userService.UpdateReputationAsync(answer.AuthorId, -oldDelta, cancellationToken);

            if (existingVote.VoteType == request.VoteType)
            {
                // Toggle off — remove vote
                _voteRepository.RemoveAnswerVote(existingVote);
            }
            else
            {
                // Change vote — apply new reputation
                existingVote.VoteType = request.VoteType;
                existingVote.UpdatedAt = DateTime.UtcNow;
                _voteRepository.UpdateAnswerVote(existingVote);
                var newDelta = request.VoteType == VoteType.Upvote ? 10 : -2;
                await _userService.UpdateReputationAsync(answer.AuthorId, newDelta, cancellationToken);
            }
        }
        else
        {
            var vote = new AnswerVote
            {
                AnswerId = request.AnswerId,
                UserId = userId,
                VoteType = request.VoteType
            };
            await _voteRepository.AddAnswerVoteAsync(vote, cancellationToken);
            var delta = request.VoteType == VoteType.Upvote ? 10 : -2;
            await _userService.UpdateReputationAsync(answer.AuthorId, delta, cancellationToken);
        }

        await _voteRepository.SaveChangesAsync(cancellationToken);

        var newScore = await _voteRepository.GetAnswerVoteScoreAsync(request.AnswerId, cancellationToken);
        return new VoteResultDto(newScore);
    }
}

// Validator
public class VoteAnswerCommandValidator : AbstractValidator<VoteAnswerCommand>
{
    public VoteAnswerCommandValidator()
    {
        RuleFor(x => x.VoteType)
            .IsInEnum().WithMessage("VoteType must be 1 (Upvote) or -1 (Downvote).");
    }
}
