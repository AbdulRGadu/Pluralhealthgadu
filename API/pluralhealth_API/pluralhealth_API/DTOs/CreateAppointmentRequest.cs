namespace pluralhealth_API.DTOs
{
    public class CreateAppointmentRequest
    {
        public int PatientId { get; set; }
        public int ClinicId { get; set; }
        public int AppointmentTypeId { get; set; }
        public DateTime StartTime { get; set; }
        public int? DurationMinutes { get; set; }
    }
}

