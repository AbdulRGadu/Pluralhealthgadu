using System.Text.Json.Serialization;

namespace pluralhealth_API.DTOs
{
    public class CreateAppointmentRequest
    {
        [JsonPropertyName("patientId")]
        public int PatientId { get; set; }
        
        [JsonPropertyName("clinicId")]
        public int ClinicId { get; set; }
        
        [JsonPropertyName("appointmentTypeId")]
        public int AppointmentTypeId { get; set; }
        
        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; }
        
        [JsonPropertyName("durationMinutes")]
        public int? DurationMinutes { get; set; }
    }
}

