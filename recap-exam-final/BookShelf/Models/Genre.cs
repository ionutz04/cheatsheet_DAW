namespace BookShelf.Models;

public class Genre : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Book> Books { get; set; } = new List<Book>();
}
