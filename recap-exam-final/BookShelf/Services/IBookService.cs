using BookShelf.Models;

namespace BookShelf.Services;

public interface IBookService
{
    Task<List<Book>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Book book, CancellationToken cancellationToken = default);
    Task UpdateAsync(Book book, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
