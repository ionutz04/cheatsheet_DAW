using BookShelf.Data;
using BookShelf.Models;
using BookShelf.Repositories;
using BookShelf.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookShelf.Tests.Services;

// Partea 2: harness-ul de referinta (verde). De aici copiem / adaptam testele scrise impreuna.
// Pattern: InMemory database + Arrange / Act / Assert + denumire Metoda_Scenariu_Rezultat.
public class BookServiceTests
{
    private static (BookService service, AppDbContext context) CreateService(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new AppDbContext(options);
        var unitOfWork = new UnitOfWork(context);
        var service = new BookService(unitOfWork, NullLogger<BookService>.Instance);
        return (service, context);
    }

    private static void SeedTwoBooks(AppDbContext context)
    {
        var genre = new Genre { Id = 1, Name = "Ficțiune" };
        var author = new Author { Id = 1, Name = "Autor Test", CreatedAt = new DateTime(2026, 1, 1) };
        context.Genres.Add(genre);
        context.Authors.Add(author);
        context.Books.AddRange(
            new Book
            {
                Id = 1,
                Title = "Carte test 1",
                Description = "Descriere suficient de lunga pentru test",
                PublishedAt = new DateTime(2026, 2, 1),
                GenreId = 1,
                AuthorId = 1
            },
            new Book
            {
                Id = 2,
                Title = "Carte test 2",
                Description = "Alta descriere suficient de lunga",
                PublishedAt = new DateTime(2026, 2, 2),
                GenreId = 1,
                AuthorId = 1
            }
        );
        context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllBooks()
    {
        var (service, context) = CreateService(nameof(GetAllAsync_ReturnsAllBooks));
        SeedTwoBooks(context);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsBook()
    {
        var (service, context) = CreateService(nameof(GetByIdAsync_ExistingId_ReturnsBook));
        SeedTwoBooks(context);

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Carte test 1", result!.Title);
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ReturnsNull()
    {
        var (service, context) = CreateService(nameof(GetByIdAsync_InvalidId_ReturnsNull));
        SeedTwoBooks(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_SetsPublishedAt()
    {
        var (service, context) = CreateService(nameof(AddAsync_SetsPublishedAt));
        SeedTwoBooks(context);

        var book = new Book
        {
            Title = "Carte noua",
            Description = "Descriere suficient de lunga pentru test",
            GenreId = 1,
            AuthorId = 1,
            PublishedAt = DateTime.MinValue
        };
        await service.AddAsync(book);

        Assert.True(book.PublishedAt > DateTime.MinValue);
    }

    [Fact]
    public async Task AddAsync_IncreasesCount()
    {
        var (service, context) = CreateService(nameof(AddAsync_IncreasesCount));
        SeedTwoBooks(context);

        var book = new Book
        {
            Title = "Carte noua",
            Description = "Descriere suficient de lunga pentru test",
            GenreId = 1,
            AuthorId = 1
        };
        await service.AddAsync(book);

        var all = await service.GetAllAsync();
        Assert.Equal(3, all.Count);
    }
}
