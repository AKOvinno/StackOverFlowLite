using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;
using StackOverflowLite.Infrastructure.Identity;
using StackOverflowLite.Infrastructure.Persistence.Context;

namespace StackOverflowLite.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IQuestionRepository.
/// Author names are resolved by joining to AspNetUsers via UserManager lookup
/// or by projecting from the Identity table directly via DbContext.Users.
/// Domain entities have no navigation properties to ApplicationUser.
/// </summary>
public class QuestionRepository : IQuestionRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public QuestionRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<QuestionDetail?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var question = await _context.Questions
            .AsNoTracking()
            .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
            .Include(q => q.Votes)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (question == null) return null;

        var authorName = await GetUserNameAsync(question.AuthorId);

        return new QuestionDetail(
            question.Id,
            question.Title,
            question.Description,
            question.AuthorId,
            authorName,
            question.ViewCount,
            question.AcceptedAnswerId,
            question.Votes.Sum(v => (int)v.VoteType),
            question.QuestionTags.Select(qt => qt.Tag.Name),
            question.CreatedAt,
            question.UpdatedAt
        );
    }

    public async Task<PagedQuestions> GetPagedAsync(
        string? keyword, string? tag, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Questions
            .AsNoTracking()
            .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
            .Include(q => q.Answers)
            .Include(q => q.Votes)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.ToLower();
            query = query.Where(q =>
                q.Title.ToLower().Contains(kw) ||
                q.Description.ToLower().Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var t = tag.ToLower();
            query = query.Where(q => q.QuestionTags.Any(qt => qt.Tag.Name.ToLower() == t));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var questions = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Batch-resolve author names from Identity store
        var authorIds = questions.Select(q => q.AuthorId).Distinct().ToList();
        var authorNames = await ResolveUserNamesAsync(authorIds);

        var items = questions.Select(q => new QuestionSummary(
            q.Id,
            q.Title,
            q.AuthorId,
            authorNames.GetValueOrDefault(q.AuthorId, q.AuthorId),
            q.ViewCount,
            q.Answers.Count,
            q.Votes.Sum(v => (int)v.VoteType),
            q.AcceptedAnswerId.HasValue,
            q.QuestionTags.Select(qt => qt.Tag.Name),
            q.CreatedAt
        ));

        return new PagedQuestions(items, totalCount);
    }

    public async Task<Question?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Questions.FindAsync(new object[] { id }, cancellationToken);

    public async Task<Question?> GetByIdWithTagsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Questions
            .Include(q => q.QuestionTags)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public async Task AddAsync(Question question, CancellationToken cancellationToken = default) =>
        await _context.Questions.AddAsync(question, cancellationToken);

    public void Update(Question question) => _context.Questions.Update(question);

    public void Remove(Question question) => _context.Questions.Remove(question);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public async Task IncrementViewCountAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        var question = await _context.Questions.FindAsync(new object[] { questionId }, cancellationToken);
        if (question == null) return;
        question.ViewCount++;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.UserName ?? userId;
    }

    private async Task<Dictionary<string, string>> ResolveUserNamesAsync(IEnumerable<string> userIds)
    {
        // Batch lookup from Identity store using DbContext.Users (same DbContext, efficient)
        var ids = userIds.ToList();
        var users = await _context.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.UserName })
            .ToListAsync();
        return users.ToDictionary(u => u.Id, u => u.UserName ?? u.Id);
    }
}
