namespace pluralhealth_API.DTOs
{
    public class CreatePatientRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public decimal WalletBalance { get; set; } = 0;
        public string Currency { get; set; } = "NGN";
    }
}

