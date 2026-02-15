namespace Populate
{
    public class Patient
    {
        public Guid? Id { get; set; }
        public Name Name { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string? Active { get; set; }
    }
}
