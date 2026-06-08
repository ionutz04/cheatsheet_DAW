using BookShelf.Data;
using BookShelf.Models;
using Microsoft.EntityFrameworkCore;

namespace BookShelf.Repositories;

public class AuthorRepository : Repository<Author>, IAuthorRepository
{
    public AuthorRepository(AppDbContext context) : base(context) { }

    public async Task<List<Author>> GetAllWithBooksAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Authors
            .Include(a => a.Books)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }
}
