namespace Acadus___Alliance_Project_2025.Models
{
    /// <summary>
    /// Model used to capture the email address for verification workflows (e.g. password reset).
    /// </summary>
    public class EmailVerificationModel
    {
        /// <summary>The email address provided by the user.</summary>
        public string Email { get; set; } = string.Empty;
    }
}








