namespace pluralhealth_UI.Services
{
    public class LoginService
    {
        // Hardcoded users for MVP
        private readonly Dictionary<string, (string Password, string Role)> _users = new()
        {
            { "admin", ("admin123", "Admin") },
            { "superadmin", ("superadmin123", "SuperAdmin") }
        };

        public bool ValidateCredentials(string username, string password, out string? role)
        {
            role = null;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            if (_users.TryGetValue(username.ToLower(), out var user))
            {
                if (user.Password == password)
                {
                    role = user.Role;
                    return true;
                }
            }

            return false;
        }

        public bool UserExists(string username)
        {
            return _users.ContainsKey(username.ToLower());
        }
    }
}

