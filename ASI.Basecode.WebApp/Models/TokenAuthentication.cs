namespace ASI.Basecode.WebApp.Models
{
    public class TokenAuthentication
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string TokenPath { get; set; } = string.Empty;
        public string CookieName { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; }
    }
}




