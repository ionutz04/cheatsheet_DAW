using Microsoft.AspNetCore.Mvc;
using StudentsApi.Controllers;
using StudentsApi.Models;
using Xunit;

namespace StudentsApi.Tests;

// Acestea sunt testele pe care le scriem IMPREUNA in sesiune, ca sa intelegem:
//  - de ce scriem teste (prind regresiile, documenteaza comportamentul);
//  - ce e un unit test (testeaza o bucata mica, izolata, fara retea/DB);
//  - pattern-ul AAA (Arrange / Act / Assert) si denumirea Metoda_Scenariu_Rezultat.
// Testam DOAR endpoint-urile read-only (Filter / Top / GetById). Lista de studenti e statica
// (partajata), deci evitam intentionat Create/Delete, care ar modifica starea intre teste.
public class StudentsControllerTests
{
    private static StudentsController CreateController() => new();

    [Fact]
    public void Filter_MinAverageNull_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Filter(null);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Filter_MinAverage8_ReturnsOnlyStudentsAtOrAbove()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Filter(8.0);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var students = Assert.IsAssignableFrom<IEnumerable<Student>>(ok.Value);
        Assert.Equal(3, students.Count()); // Ana 9.10, Ioana 8.30, Maria 9.60
        Assert.All(students, s => Assert.True(s.Average >= 8.0));
    }

    [Fact]
    public void Top_MinAverage8_OrdersByAverageDescending()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Top(8.0);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var students = Assert.IsAssignableFrom<IEnumerable<Student>>(ok.Value).ToList();
        Assert.Equal(new[] { "Maria", "Ana", "Ioana" }, students.Select(s => s.Name).ToArray());
    }

    [Fact]
    public void GetById_ExistingId_ReturnsStudent()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetById(1);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var student = Assert.IsType<Student>(ok.Value);
        Assert.Equal("Ana", student.Name);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetById(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    // ---------------------------------------------------------------------------
    // De scris IMPREUNA, live - simplu, pe COD DE STATUS (ca testele din lab10):
    // verificam DOAR ce raspuns intoarce metoda (Ok / BadRequest / NotFound),
    // fara sa ne uitam in continut. Un singur Assert pe tip.
    //
    //  - GetAll_ReturnsOk:                    GetAll()   -> OkObjectResult     (200)
    //  - Filter_ValidMin_ReturnsOk:           Filter(7)  -> OkObjectResult     (200)
    //  - Filter_AboveRange_ReturnsBadRequest: Filter(11) -> BadRequestObjectResult (400)
    //  - Stats_ReturnsOk:                     GetStats() -> OkObjectResult     (200)
    // ---------------------------------------------------------------------------

    [Fact]
    public void GetAll_ReturnsOk()
    {
        // Arrange
        var controller = CreateController();
        // Act
        var result = controller.GetAll();
        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void Filter_AboveRange_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Filter(12);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
