namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// Development/Testing user for easier access during development.
    /// Used only when DevMode is enabled in appsettings.json
    /// </summary>
    public class DevUser
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Role { get; set; } // "Student", "Teacher", "Admin"
    }
}
