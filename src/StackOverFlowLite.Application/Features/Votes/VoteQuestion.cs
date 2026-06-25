using FluentValidation;
using MediatR;
using StackOverflowLite.Application.Common.Exceptions;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;
using StackOverflowLite.Domain.Enums;

namespace StackOverflowLite.Application.Features.Votes;

// Command
public record VoteQuestionCommand(Guid QuestionId, VoteType VoteType) : IRequest<VoteResultDto>;

// Handler — no UserManager, no EF IQueryable; all through clean interfaces
public class VoteQuestionCommandHandler : IRequestHandler<VoteQuestionCommand, VoteResultDto>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IVoteRepository _voteRepository;
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public VoteQuestionCommandHandler(
        IQuestionRepository questionRepository,
        IVoteRepository voteRepository,
        IUserService userService,
        ICurrentUserService currentUserService,
        ICacheService cacheService)
    {
        _questionRepository = questionRepository;
        _voteRepository = voteRepository;
        _userService = userService;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<VoteResultDto> Handle(VoteQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenAccessException();

        var question = await _questionRepository.GetByIdAsync(request.QuestionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Question), request.QuestionId);

        if (question.AuthorId == userId)
            throw new BadRequestException("You cannot vote on your own question.");

        var existingVote = await _voteRepository.GetQuestionVoteAsync(
            request.QuestionId, userId, cancellationToken);

        if (existingVote != null)
        {
            // Reverse old reputation: upvote gave +5, downvote gave -1
            var oldDelta = existingVote.VoteType == VoteType.Upvote ? 5 : -1;
            await _userService.UpdateReputationAsync(question.AuthorId, -oldDelta, cancellationToken);

            if (existingVote.VoteType == request.VoteType)
            {
                // Same vote toggled — remove it (no new reputation)
                _voteRepository.RemoveQuestionVote(existingVote);
            }
            else
            {
                // Changed vote — apply new reputation
                existingVote.VoteType = request.VoteType;
                existingVote.UpdatedAt = DateTime.UtcNow;
                _voteRepository.UpdateQuestionVote(existingVote);
                var newDelta = request.VoteType == VoteType.Upvote ? 5 : -1;
                await _userService.UpdateReputationAsync(question.AuthorId, newDelta, cancellationToken);
            }
        }
        else
        {
            var vote = new QuestionVote
            {
                QuestionId = request.QuestionId,
                UserId = userId,
                VoteType = request.VoteType
            };
            await _voteRepository.AddQuestionVoteAsync(vote, cancellationToken);
            var delta = request.VoteType == VoteType.Upvote ? 5 : -1;
            await _userService.UpdateReputationAsync(question.AuthorId, delta, cancellationToken);
        }

        await _voteRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"question:{request.QuestionId}", cancellationToken);
        await _cacheService.RemoveByPrefixAsync("questions:", cancellationToken);

        var newScore = await _voteRepository.GetQuestionVoteScoreAsync(request.QuestionId, cancellationToken);
        return new VoteResultDto(newScore);
    }
}

// Validator
public class VoteQuestionCommandValidator : AbstractValidator<VoteQuestionCommand>
{
    public VoteQuestionCommandValidator()
    {
        RuleFor(x => x.VoteType)
            .IsInEnum().WithMessage("VoteType must be 1 (Upvote) or -1 (Downvote).");
    }
}
