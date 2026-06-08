using BookShelf.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShelf.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (context.Database.IsRelational())
            context.Database.Migrate();
        else
            context.Database.EnsureCreated();

        // Roluri
        string[] roleNames = ["Admin", "User"];
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        // Utilizator admin
        var adminEmail = "admin@bookshelf.com";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FullName = "Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Genuri, autori, carti
        if (!context.Genres.Any())
        {
            var fictiune = new Genre { Name = "Ficțiune" };
            var stiinta = new Genre { Name = "Știință" };
            var istorie = new Genre { Name = "Istorie" };
            context.Genres.AddRange(fictiune, stiinta, istorie);

            var eminescu = new Author
            {
                Name = "Mihai Eminescu",
                Bio = "Poet național, una dintre cele mai importante voci ale literaturii române.",
                CreatedAt = new DateTime(2026, 1, 10)
            };
            var sagan = new Author
            {
                Name = "Carl Sagan",
                Bio = "Astronom și popularizator al științei.",
                CreatedAt = new DateTime(2026, 1, 12)
            };
            context.Authors.AddRange(eminescu, sagan);
            await context.SaveChangesAsync();

            context.Books.AddRange(
                new Book
                {
                    Title = "Poezii alese",
                    Description = "O selecție de poezii reprezentative din lirica eminesciană.",
                    PublishedAt = new DateTime(2026, 2, 1),
                    GenreId = fictiune.Id,
                    AuthorId = eminescu.Id
                },
                new Book
                {
                    Title = "Cosmos",
                    Description = "O călătorie prin univers, de la originea vieții la marginea cunoașterii.",
                    PublishedAt = new DateTime(2026, 2, 5),
                    GenreId = stiinta.Id,
                    AuthorId = sagan.Id
                },
                new Book
                {
                    Title = "Lumea bântuită de demoni",
                    Description = "Despre gândirea critică și știința ca lumânare în întuneric.",
                    PublishedAt = new DateTime(2026, 2, 9),
                    GenreId = stiinta.Id,
                    AuthorId = sagan.Id
                }
            );
            await context.SaveChangesAsync();
        }
    }
}
