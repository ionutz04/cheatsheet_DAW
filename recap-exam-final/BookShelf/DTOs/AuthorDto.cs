namespace BookShelf.DTOs;

public record AuthorDto(
    int Id,
    string Name,
    string? Bio,
    int BookCount,
    DateTime CreatedAt);
