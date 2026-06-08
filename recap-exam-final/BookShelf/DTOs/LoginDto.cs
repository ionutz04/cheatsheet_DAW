using System.ComponentModel.DataAnnotations;

namespace BookShelf.DTOs;

public record LoginDto(
    [Required][EmailAddress] string Email,
    [Required] string Password);
