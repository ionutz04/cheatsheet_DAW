using BookShelf.Data;
using BookShelf.Models;

namespace BookShelf.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context) { }
}
