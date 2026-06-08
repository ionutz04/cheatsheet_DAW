namespace BookShelf.Models;

public class Book : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }

    // FK + navigatie catre Genre
    public int GenreId { get; set; }
    public Genre? Genre { get; set; }

    // FK + navigatie catre Author
    public int AuthorId { get; set; }
    public Author? Author { get; set; }

    public List<Review> Reviews { get; set; } = [];
}
