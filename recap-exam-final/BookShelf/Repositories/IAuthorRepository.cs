using BookShelf.Models;

namespace BookShelf.Repositories;

public interface IAuthorRepository : IRepository<Author>
{
    Task<List<Author>> GetAllWithBooksAsync(CancellationToken cancellationToken = default);
}
