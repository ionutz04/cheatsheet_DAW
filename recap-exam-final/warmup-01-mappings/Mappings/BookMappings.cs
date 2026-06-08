using MappingDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MappingDemo.Mappings;

internal static class BookMappings
{
    public static BookDto ToDto(this Book book)
    {
        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.AuthorName,
            PublishedOn = book.PublishedAt.ToString("yyyy-MM-dd")
        };
    }
}
