namespace pluralhealth_API.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        public int ClinicId { get; set; }
        public Clinic? Clinic { get; set; }
        public int AppointmentTypeId { get; set; }
        public AppointmentType? AppointmentType { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; } = "Scheduled";
        public int FacilityId { get; set; }
        public Facility? Facility { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

