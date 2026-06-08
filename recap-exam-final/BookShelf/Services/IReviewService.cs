using BookShelf.Models;

namespace BookShelf.Services;

public interface IReviewService
{
    Task<List<Review>> GetAllAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(Review review, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
