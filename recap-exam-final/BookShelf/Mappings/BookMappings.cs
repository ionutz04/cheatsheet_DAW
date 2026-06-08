using BookShelf.DTOs;
using BookShelf.Models;
using static System.Reflection.Metadata.BlobBuilder;

namespace BookShelf.Mappings;

public static class BookMappings
{
    public static BookDto ToDto(this Book book)
    {
        return new BookDto(
                    book.Id,
                    book.Title,
                    book.Description,
                    book.PublishedAt,
                    book.GenreId,
                    book.Genre?.Name ?? "N/A",
                    book.AuthorId,
                    book.Author?.Name ?? "N/A");
    }
    public static Book ToBook(this CreateBookDto dto)
    {
        return new Book
        {
            Title = dto.Title,
            Description = dto.Description,
            GenreId = dto.GenreId,
            AuthorId = dto.AuthorId
        };
    }
    public static Book ApplyTo(this UpdateBookDto dto, Book book)
    {
        book.Title = dto.Title;
        book.Description = dto.Description;
        book.GenreId = dto.GenreId;
        book.AuthorId = dto.AuthorId;
        return book;
    }
    public static List<BookDto> ToDtoList(this List<Book> books)
    {
        var dtos = new List<BookDto>();
        foreach (var book in books)
        {
            dtos.Add(book.ToDto());
        }
        return dtos;
    }

    
}
