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
        public List<PatientListItem> Patients { get; set; } = new();
    }

    public class PatientListItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public int Id { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("phone")]
        public string? Phone { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("walletBalance")]
        public decimal WalletBalance { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}

