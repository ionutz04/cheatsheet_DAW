using BookShelf.Models;

namespace BookShelf.Services;

public interface IGenreService
{
    Task<List<Genre>> GetAllAsync(CancellationToken cancellationToken = default);
}
