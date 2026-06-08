namespace BookShelf.Data;

using BookShelf.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Genre> Genres { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }

    public DbSet<Review> Reviews { get; set; }  

    // Fara configurare Fluent API: relatiile sunt deduse prin conventii EF Core
    // din proprietatile de navigatie (ex: Book.GenreId + Book.Genre + Genre.Books).
    // O entitate noua are nevoie doar de: int XId; X? X;  (+ colectia pe parinte).
}
