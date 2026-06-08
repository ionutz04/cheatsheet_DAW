using BookShelf.DTOs;
using BookShelf.Models;

namespace BookShelf.Mappings;

// Exemplu DEJA refactorizat: mapping-ul Author -> AuthorDto traieste intr-o clasa
// statica, ca metode de extensie (ToDto / ToDtoList / ToEntity). Controller-ul ramane curat.
// In Partea 1 facem EXACT acelasi lucru pentru Book (vezi BooksController, plin de mapping inline).
public static class AuthorMappings
{
    public static AuthorDto ToDto(this Author author) => new(
        Id: author.Id,
        Name: author.Name,
        Bio: author.Bio,
        BookCount: author.Books?.Count ?? 0,
        CreatedAt: author.CreatedAt);

    public static List<AuthorDto> ToDtoList(this IEnumerable<Author> authors)
        => authors.Select(a => a.ToDto()).ToList();

    public static Author ToEntity(this CreateAuthorDto dto) => new()
    {
        Name = dto.Name,
        Bio = dto.Bio
    };
}
