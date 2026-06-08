using BookShelf.Models;

namespace BookShelf.Services;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
    int ExpiresInSeconds { get; }
}
