using BookShelf.DTOs;
using BookShelf.Mappings;
using BookShelf.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookShelf.Controllers;

[ApiController]
[Route("api/genres")]
public class GenresController : ControllerBase
{
    private readonly IGenreService _genreService;

    public GenresController(IGenreService genreService)
    {
        _genreService = genreService;
    }

    // GET: /api/genres
    [HttpGet]
    public async Task<ActionResult<List<GenreDto>>> GetAll(CancellationToken cancellationToken)
    {
        var genres = await _genreService.GetAllAsync(cancellationToken);
        return Ok(genres.ToDtoList());
    }
}
