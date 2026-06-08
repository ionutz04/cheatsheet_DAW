namespace BookShelf.Models;

public class Author : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}
