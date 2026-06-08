using BookShelf.Data;
using BookShelf.Models;

namespace BookShelf.Repositories;

public class GenreRepository : Repository<Genre>, IGenreRepository
{
    public GenreRepository(AppDbContext context) : base(context) { }
}
