namespace pluralhealth_UI.Models.ViewModels
{
    public class CreateInvoiceViewModel
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;
        public decimal WalletBalance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public List<InvoiceItemViewModel> Items { get; set; } = new() { new InvoiceItemViewModel() };
        public decimal DiscountAmount { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }

    public class InvoiceItemViewModel
    {
        public string ServiceName { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}

