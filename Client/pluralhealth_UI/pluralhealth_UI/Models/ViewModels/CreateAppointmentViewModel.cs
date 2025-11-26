namespace pluralhealth_UI.Models.ViewModels
{
    public class CreateAppointmentViewModel
    {
        public int? PatientId { get; set; }
        public string? PatientSearch { get; set; }
        public List<PatientOption> Patients { get; set; } = new();
        public int ClinicId { get; set; }
        public List<ClinicOption> Clinics { get; set; } = new();
        public int AppointmentTypeId { get; set; }
        public List<AppointmentTypeOption> AppointmentTypes { get; set; } = new();
        public DateTime AppointmentDate { get; set; } = DateTime.Today;
        public TimeSpan AppointmentTime { get; set; } = DateTime.Now.TimeOfDay;
        public int DurationMinutes { get; set; } = 60;
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }

    public class PatientOption
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class AppointmentTypeOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DefaultDurationMinutes { get; set; }
    }
}

