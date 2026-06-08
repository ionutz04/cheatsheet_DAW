namespace BookShelf.Repositories;

public interface IUnitOfWork
{
    IBookRepository BookRepository { get; }
    IAuthorRepository AuthorRepository { get; }
    IGenreRepository GenreRepository { get; }
    IReviewRepository ReviewRepository { get; }
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
