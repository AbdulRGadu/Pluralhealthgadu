namespace pluralhealth_API.Models
{
    public class AppointmentType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DefaultDurationMinutes { get; set; } = 60;
        public int FacilityId { get; set; }
        public Facility? Facility { get; set; }
        
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}

