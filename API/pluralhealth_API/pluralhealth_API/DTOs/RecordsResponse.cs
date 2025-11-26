namespace pluralhealth_API.DTOs
{
    public class RecordsResponse
    {
        public int AppointmentId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;
        public DateTime AppointmentTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public decimal WalletBalance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public int? InvoiceId { get; set; }
        public string? InvoiceStatus { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}

