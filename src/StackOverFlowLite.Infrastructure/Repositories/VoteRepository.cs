using Microsoft.EntityFrameworkCore;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;
using StackOverflowLite.Domain.Enums;
using StackOverflowLite.Infrastructure.Persistence.Context;

namespace StackOverflowLite.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IVoteRepository.
/// Semantic query methods encapsulate all LINQ/EF details.
/// </summary>
public class VoteRepository : IVoteRepository
{
    private readonly ApplicationDbContext _context;

    public VoteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuestionVote?> GetQuestionVoteAsync(Guid questionId, string userId, CancellationToken cancellationToken = default) =>
        await _context.QuestionVotes
            .FirstOrDefaultAsync(v => v.QuestionId == questionId && v.UserId == userId, cancellationToken);

    public async Task<AnswerVote?> GetAnswerVoteAsync(Guid answerId, string userId, CancellationToken cancellationToken = default) =>
        await _context.AnswerVotes
            .FirstOrDefaultAsync(v => v.AnswerId == answerId && v.UserId == userId, cancellationToken);

    public async Task<int> GetQuestionVoteScoreAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        var votes = await _context.QuestionVotes
            .AsNoTracking()
            .Where(v => v.QuestionId == questionId)
            .ToListAsync(cancellationToken);
        return votes.Sum(v => (int)v.VoteType);
    }

    public async Task<int> GetAnswerVoteScoreAsync(Guid answerId, CancellationToken cancellationToken = default)
    {
        var votes = await _context.AnswerVotes
            .AsNoTracking()
            .Where(v => v.AnswerId == answerId)
            .ToListAsync(cancellationToken);
        return votes.Sum(v => (int)v.VoteType);
    }

    public async Task AddQuestionVoteAsync(QuestionVote vote, CancellationToken cancellationToken = default) =>
        await _context.QuestionVotes.AddAsync(vote, cancellationToken);

    public async Task AddAnswerVoteAsync(AnswerVote vote, CancellationToken cancellationToken = default) =>
        await _context.AnswerVotes.AddAsync(vote, cancellationToken);

    public void UpdateQuestionVote(QuestionVote vote) => _context.QuestionVotes.Update(vote);

    public void UpdateAnswerVote(AnswerVote vote) => _context.AnswerVotes.Update(vote);

    public void RemoveQuestionVote(QuestionVote vote) => _context.QuestionVotes.Remove(vote);

    public void RemoveAnswerVote(AnswerVote vote) => _context.AnswerVotes.Remove(vote);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);
}
