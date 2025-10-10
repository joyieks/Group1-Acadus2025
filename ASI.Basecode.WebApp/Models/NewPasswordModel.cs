namespace Acadus___Alliance_Project_2025.Models
{
    /// <summary>
    /// DTO for the new password form when resetting a user's password.
    /// </summary>
    public class NewPasswordModel
    {
        /// <summary>
        /// The new password to set for the account.
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmation of the new password. Should match <see cref="NewPassword"/>.
        /// </summary>
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}








