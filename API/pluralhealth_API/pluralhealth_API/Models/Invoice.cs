namespace pluralhealth_API.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        public string? InvoiceNumber { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Finalized, PartiallyPaid, Paid
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public int FacilityId { get; set; }
        public Facility? Facility { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? FinalizedAt { get; set; }
        
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}

