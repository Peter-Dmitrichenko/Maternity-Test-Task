using System.ComponentModel.DataAnnotations;
namespace DB.Models;
public class Patient
{
    [Key]
    public Guid Id { get; set; }

    public Name Name { get; set; }

    public int GenderId { get; set; }
    public int ActiveId { get; set; }

    [Required]
    public DateTime BirthDate { get; set; }
}
