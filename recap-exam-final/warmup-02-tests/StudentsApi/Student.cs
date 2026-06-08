using System.ComponentModel.DataAnnotations;

namespace StudentsApi.Models;

public enum Specialization
{
    Mathematics,
    ComputerScience,
    Physics
}

public class Student
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Name must be provided")]
    [MinLength(3)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = $"{nameof(Average)} must be between 1 and 10")]
    public double Average { get; set; }

    public Specialization Specialization { get; set; }
}
