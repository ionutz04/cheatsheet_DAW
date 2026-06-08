using BookShelf.Models;
using BookShelf.Repositories;

namespace BookShelf.Services;

public class BookService : IBookService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookService> _logger;

    public BookService(IUnitOfWork unitOfWork, ILogger<BookService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.BookRepository.GetAllWithDetailsAsync(cancellationToken);
    }

    public async Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.BookRepository.GetByIdWithDetailsAsync(id, cancellationToken);
    }

    public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        book.PublishedAt = DateTime.UtcNow;
        await _unitOfWork.BookRepository.AddAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Book created with id {BookId}", book.Id);
    }

    public async Task UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        _unitOfWork.BookRepository.Update(book);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await _unitOfWork.BookRepository.GetByIdAsync(id, cancellationToken);
        if (book != null)
        {
            _unitOfWork.BookRepository.Delete(book);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var books = await _unitOfWork.BookRepository.GetAllAsync(cancellationToken);
        return books.Count;
    }
}
