using System.ComponentModel.DataAnnotations;

namespace DB.Models
{
    public class GenderLookup
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Code { get; set; }
        public string Display { get; set; }
    }
}