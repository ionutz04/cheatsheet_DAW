using MappingDemo.Mappings;
using MappingDemo.Models;

var book = new Book
{
    Id = 1,
    Title = "Refactoring",
    AuthorName = "Martin Fowler",
    PublishedAt = new DateTime(1999, 7, 8)
};
// identificam obiectul primit, si obiectul dorit.
// obiectul primit: Book
// obiectul dorit: BookDto

//var dto = new BookDto
//{
//    Id = book.Id,
//    Title = book.Title,
//    Author = book.AuthorName,
//    PublishedOn = book.PublishedAt.ToString("yyyy-MM-dd")
//};
//Console.WriteLine(dto);

Console.WriteLine(book.ToDto());
