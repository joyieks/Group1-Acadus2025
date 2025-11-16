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
        /// Updates user password via Admin API (does not require user session)
        /// </summary>
        Task<bool> UpdateUserPasswordAdminAsync(string supabaseUserId, string newPassword);

        /// <summary>
        /// Uploads a profile image to Supabase Storage and returns the object path (bucket key)
        /// </summary>
        Task<string> UploadProfileImageAsync(string supabaseUserId, string fileName, System.IO.Stream fileStream, string contentType);

        /// <summary>
        /// Stores the profile image URL/path in the user's auth metadata
        /// </summary>
        Task<bool> SetUserProfileImageUrlAsync(string supabaseUserId, string imageUrl, string imagePath);

        /// <summary>
        /// Returns a signed URL for an object path, or a public URL if bucket is public
        /// </summary>
        Task<string> GetProfileImageUrlAsync(string objectPath, int expiresInSeconds = 3600);

        /// <summary>
        /// Gets the user's current profile image URL (generates a fresh signed URL if needed)
        /// </summary>
        Task<string> GetUserProfileImageUrlAsync(string supabaseUserId);

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
        Task<(string Role, string Name)> GetUserRoleAndNameAsync(string supabaseUserId);

        /// <summary>
        /// Signs in a user with email and password
        /// </summary>
        Task<Supabase.Gotrue.Session> SignInAsync(string email, string password);
    }
}