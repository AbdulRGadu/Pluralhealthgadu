namespace pluralhealth_API.DTOs
{
    public class AppointmentResponse
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int ClinicId { get; set; }
        public int AppointmentTypeId { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
        public int FacilityId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AppointmentListItem
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string AppointmentTypeName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}

