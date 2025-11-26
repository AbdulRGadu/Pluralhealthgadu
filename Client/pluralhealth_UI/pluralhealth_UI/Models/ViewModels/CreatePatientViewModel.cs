namespace pluralhealth_UI.Models.ViewModels
{
    public class CreatePatientViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public decimal WalletBalance { get; set; } = 0;
        public string Currency { get; set; } = "NGN";
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}

