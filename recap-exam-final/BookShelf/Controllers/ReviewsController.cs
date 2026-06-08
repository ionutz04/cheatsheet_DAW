using BookShelf.DTOs;
using BookShelf.Mappings;
using BookShelf.Models;
using BookShelf.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;

namespace BookShelf.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    // GET: /api/reviews
    [HttpGet]
    public async Task<ActionResult<List<Review>>> GetAll(CancellationToken cancellationToken)
    {
        var reviews = await _reviewService.GetAllAsync(cancellationToken);
        return Ok(reviews);
    }

    // POST: /api/reviews
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<AuthorDto>> Create(Review review, CancellationToken cancellationToken)
    {
        await _reviewService.CreateAsync(review, cancellationToken);
        // CreatedAtAction - 201 
        return Ok();
    }
}
