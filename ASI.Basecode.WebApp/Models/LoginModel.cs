namespace Acadus___Alliance_Project_2025.Models
{
    /// <summary>
    /// Simple DTO for login form input (email and password).
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// User's email address used for authentication.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's plain-text password entered on the login form.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}








