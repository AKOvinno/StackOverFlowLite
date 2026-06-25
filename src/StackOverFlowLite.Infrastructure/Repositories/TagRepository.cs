using Microsoft.EntityFrameworkCore;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Domain.Entities;
using StackOverflowLite.Infrastructure.Persistence.Context;

namespace StackOverflowLite.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ITagRepository.
/// </summary>
public class TagRepository : ITagRepository
{
    private readonly ApplicationDbContext _context;

    public TagRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Tags.FindAsync(new object[] { id }, cancellationToken);

    public async Task<Tag?> FindByNameAsync(string name, CancellationToken cancellationToken = default) =>
        await _context.Tags.FirstOrDefaultAsync(t => t.Name == name.ToLower(), cancellationToken);

    public async Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Tags.AsNoTracking().OrderBy(t => t.Name).ToListAsync(cancellationToken);

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default) =>
        await _context.Tags.AddAsync(tag, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);
}
