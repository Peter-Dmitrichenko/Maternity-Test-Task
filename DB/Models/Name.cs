using System.ComponentModel.DataAnnotations;
namespace DB.Models;
public class Name
{
    [Key]
    public Guid Id { get; set; }

    // Explicit foreign key to Patient
    public Guid PatientId { get; set; }

    // Back-reference navigation
    public Patient Patient { get; set; }

    public string? Use { get; set; }

    [Required]
    public string Family { get; set; }

    // Given names array
    public List<string> Given { get; set; } = new List<string>();
}
