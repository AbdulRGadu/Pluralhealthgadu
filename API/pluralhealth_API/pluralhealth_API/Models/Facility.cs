namespace pluralhealth_API.Models
{
    public class Facility
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Timezone { get; set; } = "Africa/Lagos";
        public string Currency { get; set; } = "NGN";
    }
}

