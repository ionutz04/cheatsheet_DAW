using BookShelf.DTOs;
using BookShelf.Exceptions;
using BookShelf.Mappings;
using BookShelf.Models;
using BookShelf.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShelf.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IBookService bookService, ILogger<BooksController> logger)
    {
        _bookService = bookService;
        _logger = logger;
    }

    // GET: /api/books
    [HttpGet]
    public async Task<ActionResult<List<BookDto>>> GetAll(CancellationToken cancellationToken)
    {
        var books = await _bookService.GetAllAsync(cancellationToken);
        return Ok(books.ToDtoList());
    }

    // GET: /api/books/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIdAsync(id, cancellationToken);
        if (book == null)
            return NotFound();

        return Ok(book.ToDto());

    }

    // POST: /api/books
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<BookDto>> Create(CreateBookDto dto, CancellationToken cancellationToken)
    {
        var book = dto.ToBook();
        await _bookService.AddAsync(book, cancellationToken);

        var created = await _bookService.GetByIdAsync(book.Id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, created?.ToDto());
    }

    // PUT: /api/books/5
    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Update(int id, UpdateBookDto dto, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIdAsync(id, cancellationToken);
        if (book == null)
            return NotFound();

        book = dto.ApplyTo(book);

        await _bookService.UpdateAsync(book, cancellationToken);

        return NoContent();
    }

    // DELETE: /api/books/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIdAsync(id, cancellationToken);
        if (book == null)
            return NotFound();

        await _bookService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
