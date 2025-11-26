namespace pluralhealth_UI.Models.ViewModels
{
    public class RecordsIndexViewModel
    {
        public List<RecordItem> Records { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? ClinicId { get; set; }
        public string? Search { get; set; }
        public List<ClinicOption> Clinics { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class RecordItem
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;
        public DateTime AppointmentTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public decimal WalletBalance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public int? InvoiceId { get; set; }
        public string? InvoiceStatus { get; set; }
    }

    public class ClinicOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

