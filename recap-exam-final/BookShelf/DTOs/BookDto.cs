namespace BookShelf.DTOs;

public record BookDto(
    int Id,
    string Title,
    string Description,
    DateTime PublishedAt,
    int GenreId,
    string GenreName,
    int AuthorId,
    string AuthorName);
