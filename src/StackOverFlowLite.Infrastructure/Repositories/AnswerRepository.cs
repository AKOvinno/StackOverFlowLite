using Microsoft.EntityFrameworkCore;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;
using StackOverflowLite.Infrastructure.Persistence.Context;

namespace StackOverflowLite.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IAnswerRepository.
/// Author names are resolved by joining to Identity's Users table via DbContext.
/// No navigation properties from Domain entities to ApplicationUser.
/// </summary>
public class AnswerRepository : IAnswerRepository
{
    private readonly ApplicationDbContext _context;

    public AnswerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Answer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Answers.FindAsync(new object[] { id }, cancellationToken);

    public async Task<IEnumerable<AnswerDetail>> GetByQuestionIdAsync(
        Guid questionId, CancellationToken cancellationToken = default)
    {
        var answers = await _context.Answers
            .AsNoTracking()
            .Include(a => a.Votes)
            .Where(a => a.QuestionId == questionId)
            .OrderByDescending(a => a.IsAccepted)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        // Batch-resolve author names from Identity table
        var authorIds = answers.Select(a => a.AuthorId).Distinct().ToList();
        var authorNames = await _context.Users
            .Where(u => authorIds.Contains(u.Id))
            .Select(u => new { u.Id, u.UserName })
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Id, cancellationToken);

        return answers.Select(a => new AnswerDetail(
            a.Id,
            a.Content,
            a.QuestionId,
            a.AuthorId,
            authorNames.GetValueOrDefault(a.AuthorId, a.AuthorId),
            a.IsAccepted,
            a.Votes.Sum(v => (int)v.VoteType),
            a.CreatedAt,
            a.UpdatedAt
        ));
    }

    public async Task AddAsync(Answer answer, CancellationToken cancellationToken = default) =>
        await _context.Answers.AddAsync(answer, cancellationToken);

    public void Update(Answer answer) => _context.Answers.Update(answer);

    public void Remove(Answer answer) => _context.Answers.Remove(answer);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);
}
