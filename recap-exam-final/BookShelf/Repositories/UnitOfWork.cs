using BookShelf.Data;

namespace BookShelf.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IBookRepository? _bookRepository;
    private IAuthorRepository? _authorRepository;
    private IGenreRepository? _genreRepository;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IBookRepository BookRepository
        => _bookRepository ??= new BookRepository(_context);

    public IAuthorRepository AuthorRepository
        => _authorRepository ??= new AuthorRepository(_context);

    public IGenreRepository GenreRepository
        => _genreRepository ??= new GenreRepository(_context);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
