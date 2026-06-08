using Microsoft.AspNetCore.Mvc;
using StudentsApi.Models;

namespace StudentsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    // Date "in memorie" - lista e statica, deci partajata intre cereri (si intre teste).
    private static readonly List<Student> students = new()
    {
        new Student { Id = 1, Name = "Ana",    Average = 9.10, Specialization = Specialization.ComputerScience },
        new Student { Id = 2, Name = "Mihai",  Average = 7.50, Specialization = Specialization.Mathematics },
        new Student { Id = 3, Name = "Ioana",  Average = 8.30, Specialization = Specialization.Physics },
        new Student { Id = 4, Name = "Andrei", Average = 6.90, Specialization = Specialization.ComputerScience },
        new Student { Id = 5, Name = "Maria",  Average = 9.60, Specialization = Specialization.Mathematics },
        new Student { Id = 6, Name = "Alex",   Average = 4.70, Specialization = Specialization.Physics }
    };

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(students);
    }

    [HttpGet("{id}")]
    public IActionResult GetById([FromRoute] int id)
    {
        Student? student = students.FirstOrDefault(s => s.Id == id);
        if (student == null)
        {
            return NotFound();
        }
        return Ok(student);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Student? student)
    {
        if (student == null)
        {
            return BadRequest("Missing body.");
        }
        if (string.IsNullOrWhiteSpace(student.Name) || student.Name.Trim().Length < 3)
        {
            return BadRequest("Name must be at least 3 characters long.");
        }

        student.Id = students.Count > 0 ? students.Max(s => s.Id) + 1 : 1;
        student.Name = student.Name.Trim();
        students.Add(student);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        Student? student = students.FirstOrDefault(s => s.Id == id);
        if (student == null)
        {
            return NotFound();
        }
        students.Remove(student);
        return NoContent();
    }

    [HttpGet("filter")]
    public IActionResult Filter([FromQuery] double? minAverage)
    {
        if (minAverage == null || minAverage < 1 || minAverage > 10)
        {
            return BadRequest("minAverage must be in range [1, 10] and not null.");
        }
        IEnumerable<Student> filtered = students.Where(s => s.Average >= minAverage);
        return Ok(filtered);
    }

    [HttpGet("top")]
    public IActionResult Top([FromQuery] double? minAverage)
    {
        if (minAverage == null || minAverage < 1 || minAverage > 10)
        {
            return BadRequest("minAverage must be in range [1, 10] and not null.");
        }

        IEnumerable<Student> top = students
            .Where(s => s.Average >= minAverage)
            .OrderByDescending(s => s.Average);

        return Ok(top);
    }

    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        bool allPassing = students.All(s => s.Average >= 5);
        bool anyComputerScience = students.Any(s => s.Specialization == Specialization.ComputerScience);

        return Ok(new { anyComputerScience, allPassing });
    }

    [HttpGet("stats-by-specialization")]
    public IActionResult StatsBySpecialization()
    {
        var stats = students
            .GroupBy(s => s.Specialization)
            .Select(group => new
            {
                Specialization = group.Key.ToString(),
                Count = group.Count(),
                Average = group.Average(s => s.Average),
                MinAverage = group.Min(s => s.Average),
                MaxAverage = group.Max(s => s.Average)
            });

        return Ok(stats);
    }
}
