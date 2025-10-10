namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// View model backing the login view including remember-me and return url.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Username or email used to identify the account.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Plain-text password submitted by the user.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Whether the user requested a persistent login cookie.
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// Optional URL to redirect to after successful login.
        /// </summary>
        public string ReturnUrl { get; set; } = string.Empty;
    }
}








