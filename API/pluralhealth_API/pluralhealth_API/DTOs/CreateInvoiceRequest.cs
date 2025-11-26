namespace pluralhealth_API.DTOs
{
    public class InvoiceItemRequest
    {
        public string ServiceName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    public class CreateInvoiceRequest
    {
        public int PatientId { get; set; }
        public int? AppointmentId { get; set; }
        public List<InvoiceItemRequest> Items { get; set; } = new();
        public decimal DiscountAmount { get; set; }
    }
}

