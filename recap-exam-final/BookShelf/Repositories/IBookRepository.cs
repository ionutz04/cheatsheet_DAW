using BookShelf.Models;

namespace BookShelf.Repositories;

public interface IBookRepository : IRepository<Book>
{
    Task<List<Book>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<Book?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
