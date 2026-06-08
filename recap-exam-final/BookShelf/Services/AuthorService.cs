using BookShelf.Models;
using BookShelf.Repositories;

namespace BookShelf.Services;

public class AuthorService : IAuthorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthorService> _logger;

    public AuthorService(IUnitOfWork unitOfWork, ILogger<AuthorService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<Author>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.AuthorRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.AuthorRepository.GetByIdAsync(id, cancellationToken); ;
    }

    public async Task CreateAsync(Author author, CancellationToken cancellationToken = default)
    {
        author.CreatedAt = DateTime.UtcNow;
        await _unitOfWork.AuthorRepository.AddAsync(author, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var authors = await _unitOfWork.AuthorRepository.GetAllAsync(cancellationToken);
        return authors.Count;
    }
}
