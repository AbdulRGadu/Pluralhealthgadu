namespace pluralhealth_API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int FacilityId { get; set; }
        public Facility? Facility { get; set; }
    }
}

