using BookShelf.DTOs;
using BookShelf.Models;

namespace BookShelf.Mappings;

public static class GenreMappings
{
    public static GenreDto ToDto(this Genre genre) => new(genre.Id, genre.Name);

    public static List<GenreDto> ToDtoList(this IEnumerable<Genre> genres)
        => genres.Select(g => g.ToDto()).ToList();
}
