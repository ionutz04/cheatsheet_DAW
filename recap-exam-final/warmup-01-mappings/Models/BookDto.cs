namespace MappingDemo.Models;

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string PublishedOn { get; set; } = string.Empty;

    public override string ToString()
        => $"BookDto {{ Id = {Id}, Title = {Title}, Author = {Author}, PublishedOn = {PublishedOn} }}";
}
