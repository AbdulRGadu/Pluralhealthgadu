namespace pluralhealth_UI.Models.ViewModels
{
    public class CreatePaymentViewModel
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public decimal InvoiceTotal { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}

