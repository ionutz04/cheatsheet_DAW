using System.ComponentModel.DataAnnotations;

namespace BookShelf.DTOs;

public record UpdateBookDto(
    [Required][StringLength(200, MinimumLength = 2)] string Title,
    [Required][MinLength(10)] string Description,
    [Required] int GenreId,
    [Required] int AuthorId);
