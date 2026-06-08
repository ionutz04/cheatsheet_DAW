using BookShelf.Data;
using BookShelf.Models;
using Microsoft.EntityFrameworkCore;

namespace BookShelf.Repositories;

public class BookRepository : Repository<Book>, IBookRepository
{
    public BookRepository(AppDbContext context) : base(context) { }

    public async Task<List<Book>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Books
            .Include(b => b.Genre)
            .Include(b => b.Author)
            .OrderByDescending(b => b.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Books
            .Include(b => b.Genre)
            .Include(b => b.Author)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }
}
