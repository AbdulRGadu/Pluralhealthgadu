namespace pluralhealth_API.Models
{
    public class Clinic
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int FacilityId { get; set; }
        public Facility? Facility { get; set; }
        
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}

