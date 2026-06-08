namespace BookShelf.Models;

//Review
//* Reviewer
//* Rating
//* Comment
//* PostedAt

// Metode:
//--- GetAll()
//--- Create()
//--- Delete()
//--> regula business: rating intre 1 si 5, status codes corecte returnate.

public class Review : BaseEntity
{
    public string Reviewer { get; set; } = null!;
    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime PostedAt { get; set; }

    public int BookId { get; set; }
    public Book? Book { get; set; }
}
