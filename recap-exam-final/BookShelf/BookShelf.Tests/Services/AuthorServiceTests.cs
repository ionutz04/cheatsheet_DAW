using BookShelf.Data;
using BookShelf.Models;
using BookShelf.Repositories;
using BookShelf.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookShelf.Tests.Services;

// Partea 3: aceste 3 teste PICA la start, pentru ca AuthorService are 3 bug-uri puse intentionat.
// Nu modificam testele - reparam codul de productie din AuthorService.
public class AuthorServiceTests
{
    private static (AuthorService service, AppDbContext context) CreateService(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new AppDbContext(options);
        var unitOfWork = new UnitOfWork(context);
        var service = new AuthorService(unitOfWork, NullLogger<AuthorService>.Instance);
        return (service, context);
    }

    private static void SeedThreeAuthors(AppDbContext context)
    {
        context.Authors.AddRange(
            new Author { Id = 1, Name = "Autor Unu", CreatedAt = new DateTime(2026, 1, 1) },
            new Author { Id = 2, Name = "Autor Doi", CreatedAt = new DateTime(2026, 1, 2) },
            new Author { Id = 3, Name = "Autor Trei", CreatedAt = new DateTime(2026, 1, 3) }
        );
        context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllAuthors()
    {
        var (service, context) = CreateService(nameof(GetAllAsync_ReturnsAllAuthors));
        SeedThreeAuthors(context);

        var result = await service.GetAllAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsAuthor()
    {
        var (service, context) = CreateService(nameof(GetByIdAsync_ExistingId_ReturnsAuthor));
        SeedThreeAuthors(context);

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Autor Unu", result!.Name);
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedAt()
    {
        // Arrange
        var (service, _) = CreateService(nameof(CreateAsync_SetsCreatedAt));

        // Act
        var author = new Author { Name = "Autor Nou", CreatedAt = DateTime.MinValue };
        await service.CreateAsync(author);

        // Assert
        Assert.True(author.CreatedAt > DateTime.MinValue);
    }
}
