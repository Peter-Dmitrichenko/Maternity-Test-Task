namespace Populate
{
    public class Name
    {
        public Guid? Id { get; set; }
        public Guid? PatientId { get; set; }
        public string Use { get; set; }
        public string Family { get; set; }
        public List<string> Given { get; set; } = new List<string>();
    }
}
