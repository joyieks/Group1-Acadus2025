using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ISupabaseAuthService
    {
        /// <summary>
        /// Creates a new user and sends confirmation + password setup emails
        /// </summary>
        Task<string> CreateUserAsync(string email, string password, string firstName, string lastName);

        /// <summary>
        /// Sends password setup email (using password reset)
        /// </summary>
        Task<bool> SendPasswordSetupEmailAsync(string email);

        /// <summary>
        /// Resends confirmation email
        /// </summary>
        Task<bool> ResendConfirmationEmailAsync(string email);

        /// <summary>
        /// Updates user password
        /// </summary>
        Task<bool> UpdateUserPasswordAsync(string supabaseUserId, string newPassword);

        /// <summary>
        /// Checks if user needs password setup
        /// </summary>
        Task<bool> NeedsPasswordSetupAsync(string supabaseUserId);

        /// <summary>
        /// Gets user metadata
        /// </summary>
        Task<Dictionary<string, object>> GetUserMetadataAsync(string supabaseUserId);

        /// <summary>
        /// Deletes a user
        /// </summary>
        Task<bool> DeleteUserAsync(string supabaseUserId);

        /// <summary>
        /// Gets user by email
        /// </summary>
        Task<SupabaseUser> GetUserByEmailAsync(string email);

        /// <summary>
        /// Gets Supabase client for authentication
        /// </summary>
        Task<Supabase.Client> GetSupabaseClientForAuthAsync();

        /// <summary>
        /// Verifies password reset token
        /// </summary>
        Task<bool> VerifyPasswordResetTokenAsync(string token);

        /// <summary>
        /// Determines user role based on Supabase user ID
        /// </summary>
        Task<string> GetUserRoleAsync(string supabaseUserId);
    }
}