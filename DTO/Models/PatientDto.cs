namespace DTO.Models;

public class PatientDTO
{
    public Guid? Id { get; set; }
    public NameDTO Name { get; set; }
    public DateTime BirthDate { get; set; }
    public string? Gender { get; set; }   // "male","female","other","unknown"
    public bool? Active { get; set; }   // "true","false"
}
