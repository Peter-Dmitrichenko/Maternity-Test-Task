using System.ComponentModel.DataAnnotations;

namespace DB.Models
{
    public class ActiveLookup
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Code { get; set; }
        public string Display { get; set; }
    }
}