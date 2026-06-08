using BookShelf.Models;

namespace BookShelf.Services;

public interface IAuthorService
{
    Task<List<Author>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task CreateAsync(Author author, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
