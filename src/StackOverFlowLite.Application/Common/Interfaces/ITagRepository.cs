using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Common.Interfaces;

/// <summary>
/// Business-oriented tag data access contract.
/// </summary>
public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tag?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Tag tag, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
