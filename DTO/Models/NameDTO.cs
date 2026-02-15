using System.ComponentModel.DataAnnotations;
namespace DTO.Models;
public class NameDTO
{
    public Guid? Id { get; set; }

    public Guid? PatientId { get; set; }

    public string Use { get; set; }

    [Required]
    public string Family { get; set; }

    public List<string> Given { get; set; } = new List<string>();
}