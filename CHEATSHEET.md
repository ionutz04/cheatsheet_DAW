# Cheatsheet Examen DAW — BookShelf ASP.NET Core

> Timp: **1h30m** | 3 cerințe | 40p total  
> Cont admin: `admin@bookshelf.com` / `Admin@123`  
> Stack: **net9.0** (start) / net8.0 (final) · **Pomelo MySQL** · EF Core · Identity · JWT · xUnit

---

## Cuprins

1. [Arhitectura proiectului](#1-arhitectura-proiectului)
2. [Comenzi esențiale](#2-comenzi-esențiale)
3. [Cerința 1 — Fix AuthorService (10p)](#3-cerința-1--fix-authorservice-10p)
4. [Cerința 2A — Mapping extension methods (10p)](#4-cerința-2a--mapping-extension-methods-10p)
5. [Cerința 2B — Scoate try/catch (10p)](#5-cerința-2b--scoate-trycatch-10p)
6. [Cerința 3 — Entitate nouă Review cap-coadă (20p)](#6-cerința-3--entitate-nouă-review-cap-coadă-20p)
7. [MySQL Docker — configurare completă](#7-mysql-docker--configurare-completă)
8. [Template generic — orice entitate nouă](#8-template-generic--orice-entitate-nouă)
9. [Repository extins — query-uri custom](#9-repository-extins--query-uri-custom)
10. [LINQ cheat sheet](#10-linq-cheat-sheet)
11. [Controller patterns — toate scenariile](#11-controller-patterns--toate-scenariile)
12. [Test patterns — toate scenariile](#12-test-patterns--toate-scenariile)
13. [Validations — referință completă](#13-validations--referință-completă)
14. [EF Core migrations — referință completă](#14-ef-core-migrations--referință-completă)
15. [Middleware — pipeline și custom middleware](#15-middleware--pipeline-și-custom-middleware)
16. [JWT și Identity — referință](#16-jwt-și-identity--referință)
17. [Erori frecvente la compilare și fix](#17-erori-frecvente-la-compilare-și-fix)
18. [Angular frontend — referință rapidă](#18-angular-frontend--referință-rapidă)

---

## 1. Arhitectura proiectului

```
BookShelf/
├── Models/          BaseEntity, Author, Book, Genre, Review, ApplicationUser
├── Data/            AppDbContext (IdentityDbContext<ApplicationUser>), SeedData
├── Migrations/      generate cu: dotnet ef migrations add <Nume>
├── Repositories/    IRepository<T>, Repository<T>
│                    IUnitOfWork, UnitOfWork   ← lazy init cu ??=
│                    IAuthorRepository / AuthorRepository
│                    IBookRepository   / BookRepository   ← Include în metode custom
│                    IGenreRepository  / GenreRepository
│                    IReviewRepository / ReviewRepository  ← de adăugat (Cerința 3)
├── Services/        IAuthorService / AuthorService    ← bug-uri (Cerința 1)
│                    IBookService   / BookService
│                    IGenreService  / GenreService
│                    IReviewService / ReviewService     ← de adăugat (Cerința 3)
│                    IJwtService    / JwtService
├── DTOs/            AuthorDto, CreateAuthorDto
│                    BookDto, CreateBookDto, UpdateBookDto
│                    GenreDto
│                    ReviewDto, CreateReviewDto         ← de adăugat (Cerința 3)
│                    LoginDto, RegisterDto
├── Mappings/        AuthorMappings, BookMappings       ← de creat BookMappings (Cerința 2)
│                    GenreMappings, ReviewMappings      ← de adăugat (Cerința 3)
├── Controllers/     AuthorsController, BooksController ← de refactorizat (Cerința 2)
│                    GenresController, ReviewsController ← de adăugat (Cerința 3)
│                    AuthController
├── Middleware/      ExceptionHandlingMiddleware
├── Exceptions/      TooManyRequestsException
└── Program.cs       DI · JWT · Swagger · middleware pipeline
```

### Fluxul unei cereri
```
HTTP Request
    → [Authorize] verificare JWT (middleware)
    → Controller  (validare DTO automată prin [ApiController], mapare DTO→Entitate)
    → Service     (logică business, aruncă excepții semantice)
    → UnitOfWork  (lazy init repo-uri, SaveChanges unic)
    → Repository  (LINQ queries, Include, EF DbSet)
    → AppDbContext → MySQL
```

### Excepții → HTTP (ExceptionHandlingMiddleware)
| Excepție C#                | HTTP | Când o arunci |
|---------------------------|------|---------------|
| `KeyNotFoundException`    | 404  | entitate inexistentă |
| `ArgumentException`       | 400  | date invalide logic |
| `UnauthorizedAccessException` | 403 | nu ai permisiune |
| `TooManyRequestsException` | 429 | rate limiting |
| orice altceva              | 500  | eroare neașteptată |

### DbContext la prima vedere
```csharp
// IdentityDbContext<ApplicationUser> furnizează tabele AspNet* automat
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Genre>  Genres  { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book>   Books   { get; set; }
    public DbSet<Review> Reviews { get; set; }   // adaugă pentru orice entitate nouă
    // Fără Fluent API — EF deduce relațiile din convenții (XId + X? pe copil, ICollection<X> pe părinte)
}
```

---

## 2. Comenzi esențiale

```bash
# ── Build & Test ──────────────────────────────────────────────
dotnet build                                  # compilare
dotnet test                                   # la start: 3 roșii / 6 verzi → după fix: 9 verzi
dotnet test --verbosity normal                # cu output detaliat

# ── Run ───────────────────────────────────────────────────────
dotnet run                                    # https://localhost:7080/ (Angular + API + Swagger)

# ── EF Core migrations ────────────────────────────────────────
dotnet ef migrations add <NumeMigratie>       # ex: AddReview, AddRatingIndex
dotnet ef migrations remove                   # șterge ultima migrație (dacă nu e aplicată)
dotnet ef database update                     # aplică migrațiile (SeedData face asta auto la startup)
dotnet ef database drop                       # șterge baza de date (reconstruită la dotnet run)
dotnet ef migrations list                     # listează migratii
dotnet ef dbcontext info                      # info despre DbContext (connection string activ)

# ── Docker MySQL ──────────────────────────────────────────────
docker compose up -d                          # pornește MySQL în background
docker compose down                           # oprește
docker compose down -v                        # oprește + șterge volumul (date pierdute!)
docker compose logs mysql                     # log-urile MySQL
docker exec -it bookshelf-mysql mysql -uroot -pBookShelf@123 -e "SHOW DATABASES;"
```

---

## 3. Cerința 1 — Fix AuthorService (10p)

> **Regulă de aur:** NU modifici testele. Repari codul de producție.  
> Fișier de reparat: `Services/AuthorService.cs`

### Ce testează fiecare test și ce bug are

| Test | Scenariul | Bug în cod | Fix |
|------|-----------|-----------|-----|
| `GetAllAsync_ReturnsAllAuthors` | returnează toți autorii | `throw new NotImplementedException()` | returnezi `GetAllAsync` din repo |
| `CreateAsync_SetsCreatedAt` | setează data creării | lipsea `author.CreatedAt = DateTime.UtcNow` | adaugi linia |
| `GetByIdAsync_ExistingId_ReturnsAuthor` | returnează autorul după id | `return null` în loc de variabila | returnezi `author` |

### ÎNAINTE (start — codul cu bug-uri)

```csharp
public async Task<List<Author>> GetAllAsync(CancellationToken cancellationToken = default)
{
    throw new NotImplementedException();   // BUG #1
}

public async Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
{
    var author = await _unitOfWork.AuthorRepository.GetByIdAsync(id, cancellationToken);
    return null;   // BUG #3 — face lookup-ul corect, dar returnează null
}

public async Task CreateAsync(Author author, CancellationToken cancellationToken = default)
{
    // BUG #2 — lipsea linia de mai jos
    await _unitOfWork.AuthorRepository.AddAsync(author, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
}
```

### DUPĂ (fix complet)

```csharp
public async Task<List<Author>> GetAllAsync(CancellationToken cancellationToken = default)
{
    return await _unitOfWork.AuthorRepository.GetAllAsync(cancellationToken);
}

public async Task<Author?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
{
    var author = await _unitOfWork.AuthorRepository.GetByIdAsync(id, cancellationToken);
    return author;   // sau scurt: return await _unitOfWork.AuthorRepository.GetByIdAsync(id, ct);
}

public async Task CreateAsync(Author author, CancellationToken cancellationToken = default)
{
    author.CreatedAt = DateTime.UtcNow;   // FIX BUG #2
    await _unitOfWork.AuthorRepository.AddAsync(author, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
}
```

### Verificare
```bash
dotnet test   # 9 verzi
```

---

## 4. Cerința 2A — Mapping extension methods (10p)

> **Modelul**: `Mappings/AuthorMappings.cs` este deja refactorizat — BookMappings urmează același tipar.

### Pasul 1 — `Mappings/BookMappings.cs` (crează fișierul)

```csharp
using BookShelf.DTOs;
using BookShelf.Models;

namespace BookShelf.Mappings;

public static class BookMappings
{
    // Entitate → DTO (citire)
    public static BookDto ToDto(this Book book) => new(
        book.Id,
        book.Title,
        book.Description,
        book.PublishedAt,
        book.GenreId,
        book.Genre?.Name ?? "N/A",
        book.AuthorId,
        book.Author?.Name ?? "N/A");

    // IEnumerable<Entitate> → List<DTO>
    public static List<BookDto> ToDtoList(this IEnumerable<Book> books)
        => books.Select(b => b.ToDto()).ToList();

    // CreateDTO → Entitate (creare)
    public static Book ToEntity(this CreateBookDto dto) => new()
    {
        Title = dto.Title,
        Description = dto.Description,
        GenreId = dto.GenreId,
        AuthorId = dto.AuthorId
    };

    // UpdateDTO → aplică pe entitate existentă (update)
    public static void ApplyTo(this UpdateBookDto dto, Book book)
    {
        book.Title = dto.Title;
        book.Description = dto.Description;
        book.GenreId = dto.GenreId;
        book.AuthorId = dto.AuthorId;
    }
}
```

### Pasul 2 — `Controllers/BooksController.cs` (curat)

```csharp
using BookShelf.DTOs;
using BookShelf.Mappings;
using BookShelf.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShelf.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IBookService bookService, ILogger<BooksController> logger)
    {
        _bookService = bookService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<BookDto>>> GetAll(CancellationToken cancellationToken)
    {
        var books = await _bookService.GetAllAsync(cancellationToken);
        return Ok(books.ToDtoList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIdAsync(id, cancellationToken);
        if (book == null) return NotFound();
        return Ok(book.ToDto());
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<BookDto>> Create(CreateBookDto dto, CancellationToken cancellationToken)
    {
        var book = dto.ToEntity();
        await _bookService.AddAsync(book, cancellationToken);
        var created = await _bookService.GetByIdAsync(book.Id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, created?.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Update(int id, UpdateBookDto dto, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIdAsync(id, cancellationToken);
        if (book == null) return NotFound();
        dto.ApplyTo(book);
        await _bookService.UpdateAsync(book, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIdAsync(id, cancellationToken);
        if (book == null) return NotFound();
        await _bookService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
```

---

## 5. Cerința 2B — Scoate try/catch (10p)

> **Alegi A sau B, NU amândouă.**  
> `ExceptionHandlingMiddleware` există deja și gestionează toate excepțiile.  
> **Scoți** toate blocurile `try { } catch { }`.  
> **Păstrezi** `if (x == null) return NotFound();` — aceea e decizie de control, nu tratare excepție.

```csharp
// ÎNAINTE:
[HttpGet]
public async Task<ActionResult<List<BookDto>>> GetAll(CancellationToken ct)
{
    try
    {
        var books = await _bookService.GetAllAsync(ct);
        // ... mapping inline
        return Ok(dtos);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Eroare la GetAll books");
        return StatusCode(500, new { message = "A aparut o eroare interna." });
    }
}

// DUPĂ (try/catch scos — middleware preia):
[HttpGet]
public async Task<ActionResult<List<BookDto>>> GetAll(CancellationToken ct)
{
    var books = await _bookService.GetAllAsync(ct);
    // ... mapping inline sau ToDto (dacă ai făcut și Cerința 2A)
    return Ok(books.Select(b => new BookDto(b.Id, b.Title, b.Description, b.PublishedAt,
        b.GenreId, b.Genre?.Name ?? "N/A", b.AuthorId, b.Author?.Name ?? "N/A")).ToList());
}
```

---

## 6. Cerința 3 — Entitate nouă Review cap-coadă (20p)

> **Checklist (11 pași):**  
> ☐ Model → ☐ colecție pe Book → ☐ DbContext → ☐ Migrație  
> ☐ IReviewRepository + ReviewRepository → ☐ IUnitOfWork + UnitOfWork  
> ☐ DTOs → ☐ ReviewMappings → ☐ IReviewService + ReviewService  
> ☐ Program.cs DI → ☐ ReviewsController

---

### Pasul 1 — `Models/Review.cs`

```csharp
namespace BookShelf.Models;

public class Review : BaseEntity
{
    public string Reviewer { get; set; } = null!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime PostedAt { get; set; }

    public int BookId { get; set; }
    public Book? Book { get; set; }
}
```

### Pasul 2 — `Models/Book.cs` — adaugă colecția

```csharp
// Adaugă în clasa Book (după celelalte proprietăți):
public List<Review> Reviews { get; set; } = [];
```

### Pasul 3 — `Data/AppDbContext.cs`

```csharp
public DbSet<Review> Reviews { get; set; }
// Nu e nevoie de Fluent API — EF deduce relația din ReviewId/Review pe copil + Reviews pe părinte
```

### Pasul 4 — Migrația

```bash
dotnet ef migrations add AddReview
# dotnet ef database update  ← opțional, SeedData face Migrate() automat la startup
```

### Pasul 5 — `Repositories/IReviewRepository.cs`

```csharp
using BookShelf.Models;

namespace BookShelf.Repositories;

public interface IReviewRepository : IRepository<Review> { }
```

### Pasul 5 — `Repositories/ReviewRepository.cs`

```csharp
using BookShelf.Data;
using BookShelf.Models;

namespace BookShelf.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context) { }
}
```

### Pasul 6 — `Repositories/IUnitOfWork.cs`

```csharp
namespace BookShelf.Repositories;

public interface IUnitOfWork
{
    IBookRepository   BookRepository   { get; }
    IAuthorRepository AuthorRepository { get; }
    IGenreRepository  GenreRepository  { get; }
    IReviewRepository ReviewRepository { get; }   // ← adaugă
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### Pasul 6 — `Repositories/UnitOfWork.cs`

```csharp
using BookShelf.Data;

namespace BookShelf.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IBookRepository?   _bookRepository;
    private IAuthorRepository? _authorRepository;
    private IGenreRepository?  _genreRepository;
    private IReviewRepository? _reviewRepository;   // ← adaugă

    public UnitOfWork(AppDbContext context) { _context = context; }

    public IBookRepository   BookRepository   => _bookRepository   ??= new BookRepository(_context);
    public IAuthorRepository AuthorRepository => _authorRepository ??= new AuthorRepository(_context);
    public IGenreRepository  GenreRepository  => _genreRepository  ??= new GenreRepository(_context);
    public IReviewRepository ReviewRepository => _reviewRepository ??= new ReviewRepository(_context); // ← adaugă

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
```

### Pasul 7 — DTOs

**`DTOs/ReviewDto.cs`**
```csharp
namespace BookShelf.DTOs;

public record ReviewDto(
    int Id,
    string Reviewer,
    int Rating,
    string? Comment,
    DateTime PostedAt,
    int BookId);
```

**`DTOs/CreateReviewDto.cs`**
```csharp
using System.ComponentModel.DataAnnotations;

namespace BookShelf.DTOs;

public record CreateReviewDto(
    [Required] string Reviewer,
    [Required][Range(1, 5)] int Rating,
    string? Comment,
    [Required] int BookId);
```

### Pasul 8 — `Mappings/ReviewMappings.cs`

```csharp
using BookShelf.DTOs;
using BookShelf.Models;

namespace BookShelf.Mappings;

public static class ReviewMappings
{
    public static ReviewDto ToDto(this Review r) => new(
        r.Id, r.Reviewer, r.Rating, r.Comment, r.PostedAt, r.BookId);

    public static List<ReviewDto> ToDtoList(this IEnumerable<Review> reviews)
        => reviews.Select(r => r.ToDto()).ToList();

    public static Review ToEntity(this CreateReviewDto dto) => new()
    {
        Reviewer = dto.Reviewer,
        Rating   = dto.Rating,
        Comment  = dto.Comment,
        BookId   = dto.BookId,
        PostedAt = DateTime.UtcNow
    };
}
```

### Pasul 9 — `Services/IReviewService.cs`

```csharp
using BookShelf.Models;

namespace BookShelf.Services;

public interface IReviewService
{
    Task<List<Review>> GetAllAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(Review review, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
```

### Pasul 9 — `Services/ReviewService.cs`

```csharp
using BookShelf.Models;
using BookShelf.Repositories;

namespace BookShelf.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

    public async Task<List<Review>> GetAllAsync(CancellationToken ct = default)
        => await _unitOfWork.ReviewRepository.GetAllAsync(ct);

    public async Task CreateAsync(Review review, CancellationToken ct = default)
    {
        if (review.Rating < 1 || review.Rating > 5)
            throw new ArgumentException("Rating-ul trebuie să fie între 1 și 5.");

        await _unitOfWork.ReviewRepository.AddAsync(review, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var review = await _unitOfWork.ReviewRepository.GetByIdAsync(id, ct);
        if (review == null)
            throw new KeyNotFoundException($"Review-ul cu id {id} nu există.");

        _unitOfWork.ReviewRepository.Delete(review);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

### Pasul 10 — `Program.cs` DI

```csharp
// Adaugă lângă celelalte AddScoped:
builder.Services.AddScoped<IReviewService, ReviewService>();
```

### Pasul 11 — `Controllers/ReviewsController.cs`

```csharp
using BookShelf.DTOs;
using BookShelf.Mappings;
using BookShelf.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShelf.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    // GET /api/reviews — public, 200
    [HttpGet]
    public async Task<ActionResult<List<ReviewDto>>> GetAll(CancellationToken ct)
    {
        var reviews = await _reviewService.GetAllAsync(ct);
        return Ok(reviews.ToDtoList());
    }

    // POST /api/reviews — autentificat, 201 / 400
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<ReviewDto>> Create(CreateReviewDto dto, CancellationToken ct)
    {
        var review = dto.ToEntity();
        await _reviewService.CreateAsync(review, ct);
        return CreatedAtAction(nameof(GetAll), new { }, review.ToDto());
    }

    // DELETE /api/reviews/{id} — Admin, 204 / 403 / 404
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _reviewService.DeleteAsync(id, ct);   // aruncă KeyNotFoundException → 404 automat
        return NoContent();
    }
}
```

### Coduri HTTP așteptate în Swagger

| Operație | Cod |
|----------|-----|
| `GET /api/reviews` | 200 |
| `POST /api/reviews` cu rating 4 (autentificat) | 201 |
| `POST /api/reviews` cu rating 9 (autentificat) | 400 |
| `POST /api/reviews` fără token | 401 |
| `DELETE /api/reviews/1` ca Admin | 204 |
| `DELETE /api/reviews/1` ca user | 403 |
| `DELETE /api/reviews/999` ca Admin | 404 |

### Verificare finală Cerința 3

```bash
dotnet build   # zero erori
dotnet test    # 9 verzi
dotnet run     # aplică migrația automat + seed
```

---

## 7. MySQL Docker — configurare completă

> **Atenție:** Proiectul `recap-exam-start` **DEJA** folosește MySQL + Pomelo (`net9.0`, `Pomelo 9.0.0`).  
> `recap-exam-final` folosește SQL Server (`net8.0`, `SqlServer 8.0.25`).  
> Dacă lucrezi din `start`, MySQL e deja configurat — pornești doar Docker.

### `docker-compose.yml` (în rădăcina `BookShelf/` sau lângă `.sln`)

```yaml
version: "3.9"
services:
  mysql:
    image: mysql:8.0
    container_name: bookshelf-mysql
    restart: unless-stopped
    environment:
      MYSQL_ROOT_PASSWORD: BookShelf@123
      MYSQL_DATABASE: BookShelfDb
    ports:
      - "3306:3306"
    volumes:
      - bookshelf-mysql-data:/var/lib/mysql

volumes:
  bookshelf-mysql-data:
```

```bash
docker compose up -d
```

### `appsettings.json` — connection string MySQL

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=BookShelfDb;User=root;Password=BookShelf@123;"
  },
  "Jwt": {
    "Key": "SuperSecretKeyThatIsAtLeast32BytesLong!!!!",
    "Issuer": "BookShelf",
    "Audience": "BookShelfUsers",
    "ExpiresInMinutes": 60
  }
}
```

### `BookShelf.csproj` — pachete pentru MySQL (net9.0)

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Exclude teste și Angular din build -->
    <Compile Remove="BookShelf.Tests\**" />
    <Content Remove="BookShelf.Tests\**" />
    <Compile Remove="bookshelf-web\**" />
    <Content Remove="bookshelf-web\**" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
    <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.9.0" />
  </ItemGroup>
</Project>
```

### `Program.cs` — UseMySql (start project — deja setat)

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
```

### Dacă migrezi de la SQL Server la MySQL

```bash
# 1. Șterge migrațiile vechi (SQL Server-specific)
rm -rf Migrations/

# 2. Regenerează pentru MySQL
dotnet ef migrations add InitialCreate

# 3. Aplică (sau lasă SeedData să facă automat la startup)
dotnet ef database update
```

### `SeedData.cs` — cum funcționează auto-migrate

```csharp
if (context.Database.IsRelational())
    context.Database.Migrate();    // aplică migratii la startup (SQL + MySQL)
else
    context.Database.EnsureCreated(); // pentru InMemory (în teste)
```

---

## 8. Template generic — orice entitate nouă

> Folosit când examenul cere să adaugi o entitate de la zero (ex: Review, Rating, Tag, Comment).  
> Urmează acești pași în ordine.

### Checklist rapid

```
1.  Models/NewEntity.cs                    ← proprietăți + FK + nav property
2.  Models/ParentEntity.cs                 ← adaugă ICollection<NewEntity>
3.  Data/AppDbContext.cs                   ← public DbSet<NewEntity> NewEntities { get; set; }
4.  dotnet ef migrations add AddNewEntity  ← generează SQL
5.  Repositories/INewEntityRepository.cs  ← : IRepository<NewEntity>
6.  Repositories/NewEntityRepository.cs   ← : Repository<NewEntity>, INewEntityRepository
7.  Repositories/IUnitOfWork.cs           ← adaugă proprietate
8.  Repositories/UnitOfWork.cs            ← câmp privat + lazy init cu ??=
9.  DTOs/NewEntityDto.cs                  ← record pentru citire
10. DTOs/CreateNewEntityDto.cs            ← record cu validări pentru creare
11. Mappings/NewEntityMappings.cs         ← ToDto, ToDtoList, ToEntity
12. Services/INewEntityService.cs         ← interfață
13. Services/NewEntityService.cs          ← implementare cu reguli business
14. Program.cs                            ← AddScoped<INewEntityService, NewEntityService>()
15. Controllers/NewEntitiesController.cs  ← [ApiController][Route("api/newentities")]
```

---

## 9. Repository extins — query-uri custom

> Când ai nevoie de mai mult decât `GetAllAsync` / `GetByIdAsync` din Repository generic.

### Extindere interfață

```csharp
public interface IReviewRepository : IRepository<Review>
{
    Task<List<Review>> GetByBookIdAsync(int bookId, CancellationToken ct = default);
    Task<double> GetAverageRatingForBookAsync(int bookId, CancellationToken ct = default);
}
```

### Implementare

```csharp
public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context) { }

    public async Task<List<Review>> GetByBookIdAsync(int bookId, CancellationToken ct = default)
        => await _context.Reviews
            .Where(r => r.BookId == bookId)
            .OrderByDescending(r => r.PostedAt)
            .ToListAsync(ct);

    public async Task<double> GetAverageRatingForBookAsync(int bookId, CancellationToken ct = default)
    {
        var ratings = await _context.Reviews
            .Where(r => r.BookId == bookId)
            .Select(r => r.Rating)
            .ToListAsync(ct);

        return ratings.Count == 0 ? 0 : ratings.Average();
    }
}
```

### Include (Eager Loading) — relații

```csharp
// Include simplu: Carte + Gen + Autor
_context.Books
    .Include(b => b.Genre)
    .Include(b => b.Author)
    .ToListAsync();

// ThenInclude: Include nested (Book → Author → Books ale acelui autor)
_context.Books
    .Include(b => b.Author)
        .ThenInclude(a => a.Books)
    .ToListAsync();

// Include cu colecție: Autor + cărțile lui + genul fiecărei cărți
_context.Authors
    .Include(a => a.Books)
        .ThenInclude(b => b.Genre)
    .ToListAsync();
```

### Repository generic — ce are deja

```csharp
// IRepository<T> / Repository<T>:
Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
Task<List<T>> GetAllAsync(CancellationToken ct = default);
Task AddAsync(T entity, CancellationToken ct = default);
void Update(T entity);      // nu e async — EF trackează entitatea
void Delete(T entity);      // nu e async — EF trackează entitatea
// Commit (SaveChanges) e pe IUnitOfWork, NU pe repository
```

---

## 10. LINQ cheat sheet

> EF Core traduce LINQ în SQL. InMemory execută LINQ în memorie. Comportament identic în teste.

### Filtrare și sortare

```csharp
// Where
var cs = students.Where(s => s.Specialization == Specialization.ComputerScience);

// OrderBy / OrderByDescending
var sorted = books.OrderBy(b => b.Title);
var recent = books.OrderByDescending(b => b.PublishedAt);

// ThenBy (sortare secundară)
var multi = books.OrderBy(b => b.AuthorId).ThenByDescending(b => b.PublishedAt);

// First / FirstOrDefault
var first = await _dbSet.FirstOrDefaultAsync(e => e.Id == id, ct);   // null dacă nu există
// var first = await _dbSet.FirstAsync(...);                          // throws dacă nu există

// Single / SingleOrDefault (dacă aștepți exact unul)
var unique = await _dbSet.SingleOrDefaultAsync(e => e.Email == email, ct);
```

### Proiecție și agregare

```csharp
// Select — proiecție
var names = authors.Select(a => a.Name).ToList();
var dtos  = books.Select(b => b.ToDto()).ToList();

// Count / Any / All
int count  = await _dbSet.CountAsync(b => b.AuthorId == 1, ct);
bool any   = await _dbSet.AnyAsync(b => b.Rating > 4, ct);
bool allOk = students.All(s => s.Average >= 5);

// Sum / Average / Min / Max
double avg  = reviews.Average(r => r.Rating);
int    sum  = reviews.Sum(r => r.Rating);
int    max  = reviews.Max(r => r.Rating);
int    min  = reviews.Min(r => r.Rating);

// GroupBy
var stats = students
    .GroupBy(s => s.Specialization)
    .Select(g => new
    {
        Spec    = g.Key.ToString(),
        Count   = g.Count(),
        Average = g.Average(s => s.Average),
        Min     = g.Min(s => s.Average),
        Max     = g.Max(s => s.Average)
    });
```

### Paginare (Skip / Take)

```csharp
// GET /api/books?page=2&pageSize=10
int page = 2, pageSize = 10;
var paged = await _context.Books
    .OrderBy(b => b.Id)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(ct);
```

### Filtrare cu query string (`[FromQuery]`)

```csharp
// Controller:
[HttpGet]
public async Task<IActionResult> GetAll(
    [FromQuery] int? authorId,
    [FromQuery] double? minRating,
    CancellationToken ct)
{
    var query = _context.Books.AsQueryable();

    if (authorId.HasValue)
        query = query.Where(b => b.AuthorId == authorId.Value);

    var result = await query.ToListAsync(ct);
    return Ok(result.ToDtoList());
}
```

---

## 11. Controller patterns — toate scenariile

### Toate codurile de retur disponibile

```csharp
return Ok(data);                                                     // 200
return Created(uri, data);                                           // 201
return CreatedAtAction(nameof(GetById), new { id = x.Id }, dto);     // 201
return NoContent();                                                  // 204
return BadRequest("mesaj");                                          // 400
return BadRequest(new { message = "..." });                          // 400 cu body
return Unauthorized();                                               // 401
return Unauthorized(new { message = "..." });                        // 401 cu body
return Forbid();                                                     // 403 (redirect la scheme)
return StatusCode(403, new { message = "..." });                     // 403 explicit
return NotFound();                                                   // 404
return NotFound(new { message = "..." });                            // 404 cu body
return Conflict(new { message = "..." });                            // 409
return StatusCode(429, new { message = "Too many requests" });       // 429
return StatusCode(500, new { message = "Eroare internă" });          // 500
```

### Route patterns

```csharp
[Route("api/books")]                 // rută fixă
[Route("api/[controller]")]          // înlocuiește [controller] cu BooksController → "books"
[HttpGet("{id:int}")]                 // constraint tip: int
[HttpGet("{id:int:min(1)}")]          // constraint compus: int ≥ 1
[HttpGet("{slug}")]                   // string simplu
[HttpGet("search/{term?}")]           // parametru opțional

// Parametri din cerere:
public IActionResult Get(
    [FromRoute] int id,              // din URL: /api/books/5
    [FromQuery] string? search,      // din query string: ?search=abc
    [FromBody] CreateBookDto dto,    // din body JSON (implicit cu [ApiController])
    [FromHeader] string? token       // din header HTTP
)
```

### [ApiController] — ce face automat

```csharp
// Cu [ApiController]:
// 1. Validarea automată a modelului → 400 dacă [Required] / [Range] / etc. nu sunt respectate
// 2. Binding-ul [FromBody] implicit pentru parametrii complexe
// 3. Răspunsuri ProblemDetails standard pentru erori

// Fără [ApiController] ar trebui să scrii manual:
if (!ModelState.IsValid) return BadRequest(ModelState);
```

### Obținerea utilizatorului curent din JWT

```csharp
using System.Security.Claims;

// În controller — User vine din HttpContext.User (claims din JWT):
var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier);  // sub (GUID)
var email    = User.FindFirstValue(ClaimTypes.Email);
var username = User.FindFirstValue(ClaimTypes.Name);
bool isAdmin = User.IsInRole("Admin");

// Dacă ai nevoie de ApplicationUser complet:
var user = await _userManager.GetUserAsync(User);
```

### Filtrare reviews după BookId (endpoint extra)

```csharp
// GET /api/reviews?bookId=3
[HttpGet]
public async Task<ActionResult<List<ReviewDto>>> GetAll(
    [FromQuery] int? bookId, CancellationToken ct)
{
    var reviews = await _reviewService.GetAllAsync(ct);

    if (bookId.HasValue)
        reviews = reviews.Where(r => r.BookId == bookId.Value).ToList();

    return Ok(reviews.ToDtoList());
}
```

### AllowAnonymous (bypass autorizare)

```csharp
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ReviewsController : ControllerBase
{
    [AllowAnonymous]   // override la nivel de action
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) { ... }

    [HttpPost]         // moștenește [Authorize] de pe controller
    public async Task<IActionResult> Create(...) { ... }
}
```

### Logging în controller/service

```csharp
private readonly ILogger<BooksController> _logger;

// Constructor injection:
public BooksController(IBookService bookService, ILogger<BooksController> logger)
{
    _bookService = bookService;
    _logger = logger;
}

// Utilizare:
_logger.LogInformation("Book created with id {BookId}", book.Id);
_logger.LogWarning("Book {BookId} not found", id);
_logger.LogError(ex, "Error on {Method} {Path}", context.Request.Method, context.Request.Path);
```

---

## 12. Test patterns — toate scenariile

### Pattern InMemory — harness comun (BookServiceTests)

```csharp
private static (BookService service, AppDbContext context) CreateService(string dbName)
{
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName: dbName)   // fiecare test = DB nouă (dbName unic)
        .Options;

    var context    = new AppDbContext(options);
    var unitOfWork = new UnitOfWork(context);
    var service    = new BookService(unitOfWork, NullLogger<BookService>.Instance);
    return (service, context);
}
```

> **IMPORTANT:** `dbName` diferit per test = stare izolată. Folosești `nameof(MetodaTest)` ca dbName.

### Seed data în test

```csharp
private static void SeedData(AppDbContext context)
{
    var genre  = new Genre  { Id = 1, Name = "Ficțiune" };
    var author = new Author { Id = 1, Name = "Test Author", CreatedAt = DateTime.UtcNow };
    context.Genres.Add(genre);
    context.Authors.Add(author);
    context.Books.AddRange(
        new Book { Id = 1, Title = "Carte 1", Description = "Desc suficient de lunga",
                   PublishedAt = DateTime.UtcNow, GenreId = 1, AuthorId = 1 },
        new Book { Id = 2, Title = "Carte 2", Description = "Alta descriere lunga",
                   PublishedAt = DateTime.UtcNow, GenreId = 1, AuthorId = 1 }
    );
    context.SaveChanges();   // sync — OK în test setup
}
```

### Toate formele de Assert (xUnit)

```csharp
// Valori
Assert.Equal(3, result.Count);
Assert.NotEqual(0, result.Count);
Assert.True(book.PublishedAt > DateTime.MinValue);
Assert.False(result.IsEmpty);

// Null
Assert.Null(result);
Assert.NotNull(result);

// Tipuri de rezultate din controller
Assert.IsType<OkObjectResult>(result);
Assert.IsType<NotFoundResult>(result);
Assert.IsType<BadRequestObjectResult>(result);
Assert.IsType<CreatedAtActionResult>(result);
Assert.IsType<NoContentResult>(result);

// Extragere valoare din OkObjectResult
var ok      = Assert.IsType<OkObjectResult>(result);
var student = Assert.IsType<Student>(ok.Value);
Assert.Equal("Ana", student.Name);

// Colecții
Assert.Empty(list);
Assert.NotEmpty(list);
Assert.Equal(new[] { "Maria", "Ana", "Ioana" }, students.Select(s => s.Name).ToArray());
Assert.All(students, s => Assert.True(s.Average >= 8.0));   // toți să treacă condiția
Assert.Contains(students, s => s.Name == "Ana");

// Excepții
await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(badReview));
await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(999));
Assert.Throws<NotImplementedException>(() => service.GetAll());  // sync

// IsAssignableFrom (pentru interfeșe / tipuri de bază)
var students = Assert.IsAssignableFrom<IEnumerable<Student>>(ok.Value);
```

### Test care verifică exception aruncată

```csharp
[Fact]
public async Task CreateAsync_InvalidRating_ThrowsArgumentException()
{
    var (service, _) = CreateService(nameof(CreateAsync_InvalidRating_ThrowsArgumentException));

    var review = new Review { Reviewer = "Test", Rating = 10, BookId = 1 };

    await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(review));
}
```

### Test pe controller direct (fără DB — StudentsController style)

```csharp
// Controller cu date statice/in-memorie — nu necesită InMemory DB
private static StudentsController CreateController() => new();

[Fact]
public void Filter_MinAverageNull_ReturnsBadRequest()
{
    var controller = CreateController();
    var result = controller.Filter(null);
    Assert.IsType<BadRequestObjectResult>(result);
}
```

### xUnit Theory — același test cu mai multe inputuri

```csharp
[Theory]
[InlineData(0)]    // sub minim
[InlineData(6)]    // peste maxim
[InlineData(-1)]   // negativ
public async Task CreateAsync_OutOfRangeRating_ThrowsArgumentException(int rating)
{
    var (service, _) = CreateService($"Rating_{rating}");
    var review = new Review { Reviewer = "Test", Rating = rating, BookId = 1 };
    await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(review));
}

[Theory]
[InlineData(1)]
[InlineData(3)]
[InlineData(5)]
public async Task CreateAsync_ValidRating_DoesNotThrow(int rating)
{
    var (service, context) = CreateService($"ValidRating_{rating}");
    // seed book
    context.Books.Add(new Book { Id = 1, Title = "T", Description = "Descriere lunga",
                                  GenreId = 0, AuthorId = 0 });
    context.SaveChanges();

    var review = new Review { Reviewer = "Test", Rating = rating, BookId = 1 };
    var ex = await Record.ExceptionAsync(() => service.CreateAsync(review));
    Assert.Null(ex);
}
```

### Denumire convenție teste

```
MetodaTestată_Scenariu_RezultatAșteptat
GetAllAsync_EmptyDatabase_ReturnsEmptyList
GetByIdAsync_ExistingId_ReturnsEntity
GetByIdAsync_InvalidId_ReturnsNull
CreateAsync_ValidEntity_IncreasesCount
CreateAsync_InvalidRating_ThrowsArgumentException
DeleteAsync_ExistingId_DecreasesCount
DeleteAsync_NonExistingId_ThrowsKeyNotFoundException
```

### `BookShelf.Tests.csproj` — pachete necesare

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>    <!-- sau net8.0, să corespundă cu main proj -->
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector"             Version="6.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk"         Version="17.8.0" />
    <PackageReference Include="xunit"                          Version="2.5.3" />
    <PackageReference Include="xunit.runner.visualstudio"      Version="2.5.3" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />   <!-- Fact, Theory, Assert disponibile fără using -->
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\BookShelf.csproj" />
  </ItemGroup>
</Project>
```

---

## 13. Validations — referință completă

### DataAnnotations — toate atributele utile

```csharp
using System.ComponentModel.DataAnnotations;

public record CreateEntityDto(
    [Required]                              string Name,            // nu null/gol
    [Required][StringLength(200, MinimumLength = 2)] string Title,  // lungime între 2 și 200
    [Required][MinLength(10)]               string Description,     // minim 10 caractere
    [Required][MaxLength(500)]              string Bio,             // maxim 500 caractere
    [Required][Range(1, 5)]                 int Rating,             // număr întreg 1..5
    [Required][Range(0.0, 10.0)]            double Average,         // double 0..10
    [Required][EmailAddress]                string Email,           // format email
    [Required][MinLength(6)]                string Password,        // parola min 6 chars
    [Required][Url]                         string? ImageUrl,       // URL valid
    [Required][Phone]                       string? Phone,          // format telefon
    int? OptionalId                                                  // opțional — fără [Required]
);
```

### Unde se validează automat

```csharp
// [ApiController] pe controller → validare automată la orice POST/PUT/PATCH
// Dacă ModelState.IsValid == false → răspuns 400 automat, fără cod extra

// Echivalent manual (fără [ApiController]):
if (!ModelState.IsValid)
    return BadRequest(ModelState);
```

### record vs class pentru DTOs

```csharp
// record — imutabil, egalitate structurală, pozițional:
public record BookDto(int Id, string Title, string Description, DateTime PublishedAt,
    int GenreId, string GenreName, int AuthorId, string AuthorName);

// record cu inițializatori nominali (când sunt multe proprietăți):
public record CreateBookDto(
    [Required][StringLength(200, MinimumLength = 2)] string Title,
    [Required][MinLength(10)]                         string Description,
    [Required] int GenreId,
    [Required] int AuthorId);

// Creare:
var dto = new CreateBookDto(Title: "Cosmos", Description: "O descriere suficient de lunga",
                            GenreId: 1, AuthorId: 2);

// class DTO (mai clasic, mai verbos):
public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}
```

### Custom validation attribute

```csharp
public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        if (value is DateTime dt && dt > DateTime.UtcNow)
            return ValidationResult.Success;
        return new ValidationResult("Data trebuie să fie în viitor.");
    }
}

// Utilizare:
public record CreateEventDto([Required][FutureDate] DateTime EventDate);
```

---

## 14. EF Core migrations — referință completă

### Comenzi migrations

```bash
dotnet ef migrations add <Nume>          # creează fișiere în Migrations/
dotnet ef migrations remove              # șterge ultima (dacă NU e aplicată)
dotnet ef migrations list                # listează cu statut Applied/Pending
dotnet ef database update                # aplică toate Pending
dotnet ef database update <Nume>         # aplică până la migrația numită
dotnet ef database update 0              # revert ALL (baza de date goală)
dotnet ef database drop                  # șterge baza complet
dotnet ef dbcontext info                 # arată connection string activ
dotnet ef dbcontext scaffold ...         # reverse engineering (DB → modele)
```

### Migrația generată — structură

```csharp
public partial class AddReview : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Reviews",
            columns: table => new
            {
                Id         = table.Column<int>(nullable: false)
                                  .Annotation("MySql:ValueGenerationStrategy",
                                               MySqlValueGenerationStrategy.IdentityColumn),
                Reviewer   = table.Column<string>(nullable: false),
                Rating     = table.Column<int>(nullable: false),
                Comment    = table.Column<string>(nullable: true),
                PostedAt   = table.Column<DateTime>(nullable: false),
                BookId     = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Reviews", x => x.Id);
                table.ForeignKey(
                    name: "FK_Reviews_Books_BookId",
                    column: x => x.BookId,
                    principalTable: "Books",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_Reviews_BookId", "Reviews", "BookId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Reviews");
    }
}
```

### Probleme frecvente cu migrațiile

| Problemă | Cauza | Fix |
|----------|-------|-----|
| "Already an object named..." | Migrație aplicată, dai update din nou | `dotnet ef database drop` + `dotnet run` |
| "No migrations have been applied" | Fișiere migrație există dar nu s-au aplicat | `dotnet ef database update` sau lasă SeedData |
| "Cannot connect to DB" | MySQL nu rulează | `docker compose up -d` |
| "Table 'X' doesn't exist" | Lipsă migrație sau DbSet | Adaugă `DbSet<X>` + `dotnet ef migrations add AddX` |
| "The model has pending changes" | Ai modificat entitatea fără migrație | `dotnet ef migrations add <Nume>` |
| Migrație SQL Server în proiect MySQL | Ai copiat din `recap-exam-final` | Șterge Migrations/, regenerează cu Pomelo |

### InMemory în teste — NE aplică migrații!

```csharp
// .UseInMemoryDatabase() creează schema din model, NU din migrații
// → testele merg chiar dacă nu ai nicio migrație sau dacă migrația e SQL Server-specifică
// → SeedData face IsRelational() = false → EnsureCreated() în loc de Migrate()
```

---

## 15. Middleware — pipeline și custom middleware

### Ordinea corectă în `Program.cs`

```csharp
// ORDINEA CONTEAZĂ — fiecare UseXxx() adaugă un strat
var app = builder.Build();

// 1. Seed (nu e middleware, dar înainte de Run)
using (var scope = app.Services.CreateScope())
    await SeedData.InitializeAsync(scope.ServiceProvider);

// 2. Swagger (doar Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                             c.RoutePrefix = "swagger"; });
}

// 3. Custom exception middleware (PRIMUL — ca să prindă excepțiile din tot restul)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 4. HTTPS redirect
app.UseHttpsRedirection();

// 5. Fișiere statice (Angular build în wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

// 6. Auth (Authentication ÎNAINTE de Authorization!)
app.UseAuthentication();
app.UseAuthorization();

// 7. Controllers
app.MapControllers();

// 8. Fallback pentru Angular routing (SPA)
app.MapFallbackToFile("index.html");

app.Run();
```

### Custom middleware — template complet

```csharp
public class MyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MyMiddleware> _logger;

    public MyMiddleware(RequestDelegate next, ILogger<MyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Cod înainte de next (pre-processing)
        _logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);

        try
        {
            await _next(context);   // pasează cererea mai departe
        }
        catch (Exception ex)
        {
            // Prinde excepția și o gestionează
            await HandleExceptionAsync(context, ex);
        }

        // Cod după next (post-processing) — dacă nu s-a trimis răspunsul deja
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            KeyNotFoundException        => (404, "Not found."),
            ArgumentException           => (400, "Bad request."),
            UnauthorizedAccessException => (403, "Forbidden."),
            _                           => (500, "Internal server error.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = statusCode;
        return context.Response.WriteAsJsonAsync(new { success = false, message });
    }
}

// Înregistrare în Program.cs:
app.UseMiddleware<MyMiddleware>();
```

### ExceptionHandlingMiddleware — cod complet (deja în proiect)

```csharp
// Middleware/ExceptionHandlingMiddleware.cs — deja există
// Mapeaza excepții → HTTP:
var (statusCode, message) = exception switch
{
    TooManyRequestsException    => (429, exception.Message),
    KeyNotFoundException        => (404, "Resursa nu a fost gasita."),
    UnauthorizedAccessException => (403, "Acces interzis."),
    ArgumentException           => (400, "Cerere invalida."),
    _                           => (500, "A aparut o eroare interna.")
};
```

---

## 16. JWT și Identity — referință

### Cum funcționează autentificarea JWT în acest proiect

```
1. POST /api/auth/login  → verifică email+parolă → returnează { token, expiresIn }
2. Client stochează token-ul în localStorage
3. Cereri autorizate: Header "Authorization: Bearer <token>"
4. Middleware JWT verifică semnătura + expiresa automat
5. [Authorize] pe controller blochează cererile fără token valid → 401
6. [Authorize(Roles = "Admin")] verifică claim-ul de rol → 403 dacă lipsește
```

### Configurare JWT în `Program.cs`

```csharp
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
```

### Generarea token-ului (JwtService)

```csharp
// Claims din user + roluri
var claims = new List<Claim>
{
    new(ClaimTypes.NameIdentifier, user.Id),    // sub
    new(ClaimTypes.Name, user.UserName!),
    new(ClaimTypes.Email, user.Email!)
};
var roles = await _userManager.GetRolesAsync(user);
foreach (var role in roles)
    claims.Add(new Claim(ClaimTypes.Role, role));  // role claim → [Authorize(Roles = "Admin")]

var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
var token = new JwtSecurityToken(
    issuer:             config["Jwt:Issuer"],
    audience:           config["Jwt:Audience"],
    claims:             claims,
    expires:            DateTime.UtcNow.AddMinutes(config.GetValue<int>("Jwt:ExpiresInMinutes")),
    signingCredentials: credentials);

return new JwtSecurityTokenHandler().WriteToken(token);
```

### Identity — configurare parole

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit           = false;  // nu cere cifre
    options.Password.RequireUppercase       = false;  // nu cere majuscule
    options.Password.RequireNonAlphanumeric = false;  // nu cere caractere speciale
    options.Password.RequiredLength         = 6;      // minim 6 caractere
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
```

### Operații cu UserManager

```csharp
// Caută user
var user = await _userManager.FindByEmailAsync(dto.Email);
var user = await _userManager.FindByIdAsync(userId);
var user = await _userManager.GetUserAsync(User);  // din HttpContext.User

// Creează user
var result = await _userManager.CreateAsync(user, dto.Password);
if (!result.Succeeded)
    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

// Adaugă rol
await _userManager.AddToRoleAsync(user, "User");

// Verifică parolă (fără login session)
var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);

// Roluri user
var roles = await _userManager.GetRolesAsync(user);
bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
```

### Seed admin + roluri (SeedData.cs — pattern)

```csharp
// Roluri
string[] roleNames = ["Admin", "User"];
foreach (var roleName in roleNames)
    if (!await roleManager.RoleExistsAsync(roleName))
        await roleManager.CreateAsync(new IdentityRole(roleName));

// Admin user
var adminEmail = "admin@bookshelf.com";
if (await userManager.FindByEmailAsync(adminEmail) is null)
{
    var admin = new ApplicationUser
    {
        UserName = "admin", Email = adminEmail, FullName = "Administrator", EmailConfirmed = true
    };
    var result = await userManager.CreateAsync(admin, "Admin@123");
    if (result.Succeeded)
        await userManager.AddToRoleAsync(admin, "Admin");
}
```

### `appsettings.json` — secțiunea JWT

```json
"Jwt": {
  "Key": "SuperSecretKeyThatIsAtLeast32BytesLong!!!!",
  "Issuer": "BookShelf",
  "Audience": "BookShelfUsers",
  "ExpiresInMinutes": 60
}
```

> **Key trebuie să fie minim 32 de bytes** (32 caractere ASCII = 256 biți pentru HS256).

---

## 17. Erori frecvente la compilare și fix

### CS0246 — tip negăsit (using lipsă)

```csharp
// Eroare: The type or namespace name 'Review' could not be found
// Fix: adaugă using
using BookShelf.Models;
using BookShelf.DTOs;
using BookShelf.Mappings;
using BookShelf.Repositories;
using BookShelf.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
```

### CS0535 — interfață neimplementată

```csharp
// Eroare: 'UnitOfWork' does not implement interface member 'IUnitOfWork.ReviewRepository'
// Fix: adaugă câmpul privat + proprietatea publică în UnitOfWork
private IReviewRepository? _reviewRepository;
public IReviewRepository ReviewRepository => _reviewRepository ??= new ReviewRepository(_context);
```

### CS8618 — nullable property neassigned

```csharp
// Eroare: Non-nullable property 'Name' must contain a non-null value
// Fix 1: inițializator
public string Name { get; set; } = string.Empty;
// Fix 2: null-forgiving (dacă sigur vine din DB/EF)
public string Name { get; set; } = null!;
// Fix 3: nullable (dacă poate fi null)
public string? Name { get; set; }
```

### EF Core — "Include" nedefinit

```csharp
// Eroare: 'IQueryable<Book>' does not contain a definition for 'Include'
// Fix: adaugă using
using Microsoft.EntityFrameworkCore;
```

### Migrație — "No DbContext was found"

```bash
# Fix: rulezi din folderul cu .csproj (nu din solution root)
cd recap-exam-start/BookShelf
dotnet ef migrations add AddReview
```

### "Cannot create scoped service from singleton"

```csharp
// Eroare la startup — înregistrezi un Scoped service într-un Singleton
// Fix: folosești IServiceScopeFactory în loc de injecție directă, sau schimbi lifetime-ul
// Serviciile în acest proiect sunt toate Scoped → fără problemă normală
```

### Test — "Cannot access a disposed context"

```csharp
// Cauza: folosești același context în mai multe teste (dbName identic)
// Fix: dbName unic per test cu nameof()
var (service, context) = CreateService(nameof(TestMethodName));
```

### Program.cs — "Authentication scheme already registered"

```csharp
// Cauza: AddAuthentication().AddJwtBearer() apelat de două ori
// Fix: șterge unul dintre cele două apeluri
```

---

## 18. Angular frontend — referință rapidă

> Frontend-ul este compilat în `wwwroot/` și servit de `app.UseStaticFiles()`.  
> `dotnet run` → `https://localhost:7080/` → Angular SPA.  
> **Nu trebuie să modifici Angular pentru examen** — dar e util să știi structura.

### Structura Angular (`bookshelf-web/src/app/`)

```
app/
├── core/
│   ├── api/
│   │   ├── authors.api.ts    ← HttpClient calls la /api/authors
│   │   ├── books.api.ts      ← HttpClient calls la /api/books
│   │   ├── genres.api.ts     ← HttpClient calls la /api/genres
│   │   └── reviews.api.ts    ← HttpClient calls la /api/reviews (de adăugat)
│   └── auth/
│       ├── auth.service.ts   ← login, logout, stochează JWT în localStorage
│       ├── auth.interceptor.ts ← adaugă Authorization: Bearer <token> la toate cererile
│       ├── auth.guard.ts     ← protejează rute care necesită autentificare
│       └── admin.guard.ts    ← protejează rute care necesită rol Admin
├── pages/
│   ├── books/                ← BooksComponent, BookDetailComponent
│   ├── authors/              ← AuthorsComponent
│   ├── genres/               ← GenresComponent
│   ├── home/                 ← HomeComponent
│   └── login/                ← LoginComponent
└── app.routes.ts             ← configurarea rutelor Angular
```

### `proxy.conf.json` — dev proxy (pentru `ng serve`)

```json
{
  "/api": {
    "target": "https://localhost:7080",
    "secure": false
  }
}
```

### Cum adaugi o nouă pagină în Angular (referință)

```typescript
// 1. Generează componenta
// ng generate component pages/reviews

// 2. Adaugă ruta în app.routes.ts:
{ path: 'reviews', component: ReviewsComponent },

// 3. Adaugă link în navbar (app.component.html):
// <a routerLink="/reviews">Reviews</a>
```

### Auth interceptor — cum funcționează

```typescript
// auth.interceptor.ts — adaugă token la FIECARE cerere HTTP
intercept(req: HttpRequest<any>, next: HttpHandler) {
    const token = localStorage.getItem('token');
    if (token) {
        req = req.clone({
            setHeaders: { Authorization: `Bearer ${token}` }
        });
    }
    return next.handle(req);
}
```

---

## Referință rapidă finală — pattern-uri de lipit direct

### Model nou (copy-paste)
```csharp
public class NewEntity : BaseEntity
{
    public string Field1 { get; set; } = null!;
    public int NumericField { get; set; }
    public string? OptionalField { get; set; }
    public DateTime CreatedAt { get; set; }

    public int ParentId { get; set; }   // FK
    public Parent? Parent { get; set; } // nav property
}
// Pe parent: public List<NewEntity> NewEntities { get; set; } = [];
// Pe DbContext: public DbSet<NewEntity> NewEntities { get; set; }
```

### DTO pair (copy-paste)
```csharp
public record NewEntityDto(int Id, string Field1, int NumericField, string? OptionalField, DateTime CreatedAt, int ParentId);

public record CreateNewEntityDto(
    [Required] string Field1,
    [Required][Range(1, 100)] int NumericField,
    string? OptionalField,
    [Required] int ParentId);
```

### Mappings (copy-paste)
```csharp
public static class NewEntityMappings
{
    public static NewEntityDto ToDto(this NewEntity e) => new(
        e.Id, e.Field1, e.NumericField, e.OptionalField, e.CreatedAt, e.ParentId);

    public static List<NewEntityDto> ToDtoList(this IEnumerable<NewEntity> list)
        => list.Select(e => e.ToDto()).ToList();

    public static NewEntity ToEntity(this CreateNewEntityDto dto) => new()
    {
        Field1 = dto.Field1, NumericField = dto.NumericField,
        OptionalField = dto.OptionalField, ParentId = dto.ParentId,
        CreatedAt = DateTime.UtcNow
    };
}
```

### Service complet (copy-paste)
```csharp
public class NewEntityService : INewEntityService
{
    private readonly IUnitOfWork _uow;
    public NewEntityService(IUnitOfWork uow) { _uow = uow; }

    public async Task<List<NewEntity>> GetAllAsync(CancellationToken ct = default)
        => await _uow.NewEntityRepository.GetAllAsync(ct);

    public async Task<NewEntity?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _uow.NewEntityRepository.GetByIdAsync(id, ct);

    public async Task CreateAsync(NewEntity entity, CancellationToken ct = default)
    {
        // validări business
        entity.CreatedAt = DateTime.UtcNow;
        await _uow.NewEntityRepository.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _uow.NewEntityRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Entitate cu id {id} nu există.");
        _uow.NewEntityRepository.Delete(entity);
        await _uow.SaveChangesAsync(ct);
    }
}
```

### Controller complet (copy-paste)
```csharp
[ApiController]
[Route("api/newentities")]
public class NewEntitiesController : ControllerBase
{
    private readonly INewEntityService _service;
    public NewEntitiesController(INewEntityService service) { _service = service; }

    [HttpGet]
    public async Task<ActionResult<List<NewEntityDto>>> GetAll(CancellationToken ct)
        => Ok((await _service.GetAllAsync(ct)).ToDtoList());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NewEntityDto>> GetById(int id, CancellationToken ct)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        return entity == null ? NotFound() : Ok(entity.ToDto());
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<NewEntityDto>> Create(CreateNewEntityDto dto, CancellationToken ct)
    {
        var entity = dto.ToEntity();
        await _service.CreateAsync(entity, ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
```

### Test harness (copy-paste)
```csharp
public class NewEntityServiceTests
{
    private static (NewEntityService service, AppDbContext ctx) Setup(string db)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(db).Options;
        var ctx = new AppDbContext(opts);
        var uow = new UnitOfWork(ctx);
        return (new NewEntityService(uow), ctx);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDb_ReturnsEmptyList()
    {
        var (svc, _) = Setup(nameof(GetAllAsync_EmptyDb_ReturnsEmptyList));
        var result = await svc.GetAllAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_ValidEntity_IncreasesCount()
    {
        var (svc, _) = Setup(nameof(CreateAsync_ValidEntity_IncreasesCount));
        await svc.CreateAsync(new NewEntity { Field1 = "test", NumericField = 5, ParentId = 0 });
        var all = await svc.GetAllAsync();
        Assert.Single(all);
    }

    [Fact]
    public async Task DeleteAsync_NonExisting_ThrowsKeyNotFound()
    {
        var (svc, _) = Setup(nameof(DeleteAsync_NonExisting_ThrowsKeyNotFound));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.DeleteAsync(999));
    }
}
```
