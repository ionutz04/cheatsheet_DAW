using System.ComponentModel.DataAnnotations;

namespace BookShelf.DTOs;

public record CreateAuthorDto(
    [Required][StringLength(120, MinimumLength = 2)] string Name,
    string? Bio);
