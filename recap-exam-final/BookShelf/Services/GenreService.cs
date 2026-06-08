using BookShelf.Models;
using BookShelf.Repositories;

namespace BookShelf.Services;

public class GenreService : IGenreService
{
    private readonly IUnitOfWork _unitOfWork;

    public GenreService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Genre>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.GenreRepository.GetAllAsync(cancellationToken);
    }
}
