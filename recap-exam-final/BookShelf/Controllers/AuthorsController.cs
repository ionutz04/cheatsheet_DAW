using BookShelf.DTOs;
using BookShelf.Mappings;
using BookShelf.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShelf.Controllers;

// Controller DEJA refactorizat: foloseste AuthorMappings (ToDto / ToDtoList / ToEntity).
// E modelul dupa care refactorizam BooksController in Partea 1.
[ApiController]
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorsController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    // GET: /api/authors
    [HttpGet]
    public async Task<ActionResult<List<AuthorDto>>> GetAll(CancellationToken cancellationToken)
    {
        var authors = await _authorService.GetAllAsync(cancellationToken);
        return Ok(authors.ToDtoList());
    }

    // GET: /api/authors/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuthorDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var author = await _authorService.GetByIdAsync(id, cancellationToken);
        if (author == null)
            return NotFound();

        return Ok(author.ToDto());
    }

    // POST: /api/authors
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<AuthorDto>> Create(CreateAuthorDto dto, CancellationToken cancellationToken)
    {
        var author = dto.ToEntity();
        await _authorService.CreateAsync(author, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = author.Id }, author.ToDto());
    }
}
