using System.Text.Json.Serialization;

namespace pluralhealth_UI.Models.ViewModels
{
    public class PaidInvoicesViewModel
    {
        public List<PaidInvoiceItem> Invoices { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class PaidInvoiceItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("invoiceNumber")]
        public string? InvoiceNumber { get; set; }

        [JsonPropertyName("patientId")]
        public int PatientId { get; set; }

        [JsonPropertyName("patientName")]
        public string PatientName { get; set; } = string.Empty;

        [JsonPropertyName("patientCode")]
        public string PatientCode { get; set; } = string.Empty;

        [JsonPropertyName("appointmentId")]
        public int? AppointmentId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("discountAmount")]
        public decimal DiscountAmount { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "NGN";

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("finalizedAt")]
        public DateTime? FinalizedAt { get; set; }

        [JsonPropertyName("totalPaid")]
        public decimal TotalPaid { get; set; }

        [JsonPropertyName("paymentCount")]
        public int PaymentCount { get; set; }
    }
}

