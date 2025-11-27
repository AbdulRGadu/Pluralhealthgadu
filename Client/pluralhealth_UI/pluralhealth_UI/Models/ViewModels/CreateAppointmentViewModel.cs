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
        public List<AppointmentListItem> Appointments { get; set; } = new();
    }

    public class AppointmentListItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public int Id { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("patientName")]
        public string PatientName { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("patientCode")]
        public string PatientCode { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("clinicName")]
        public string ClinicName { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("appointmentTypeName")]
        public string AppointmentTypeName { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("durationMinutes")]
        public int DurationMinutes { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
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

