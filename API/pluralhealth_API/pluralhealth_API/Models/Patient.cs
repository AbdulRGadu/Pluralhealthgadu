namespace pluralhealth_API.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public decimal WalletBalance { get; set; }
        public string Currency { get; set; } = "NGN";
        public int FacilityId { get; set; }
        public Facility? Facility { get; set; }
        public string Status { get; set; } = "Processing";
        
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}

