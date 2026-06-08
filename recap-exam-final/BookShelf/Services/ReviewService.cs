using BookShelf.Models;
using BookShelf.Repositories;

namespace BookShelf.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task CreateAsync(Review review, CancellationToken cancellationToken = default)
    {
        if (review.Rating < 1 || review.Rating > 5)
            throw new Exception();
        await _unitOfWork.ReviewRepository.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var toDelete = await _unitOfWork.ReviewRepository.GetByIdAsync(id, cancellationToken);
        if(toDelete != null)
        {
            _unitOfWork.ReviewRepository.Delete(toDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<Review>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ReviewRepository.GetAllAsync(cancellationToken);
    }
}
