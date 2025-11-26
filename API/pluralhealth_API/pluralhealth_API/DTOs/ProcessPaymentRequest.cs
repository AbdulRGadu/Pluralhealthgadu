namespace pluralhealth_API.DTOs
{
    public class ProcessPaymentRequest
    {
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
    }
}

