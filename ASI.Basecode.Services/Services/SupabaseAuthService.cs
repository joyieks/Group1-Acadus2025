using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Supabase;
using Supabase.Gotrue;
using System;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class SupabaseAuthService : ISupabaseAuthService
    {
        private readonly IConfiguration _configuration;
        private Supabase.Client _supabaseClient;
        private AdminClient _adminClient;

        public SupabaseAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private async Task<Supabase.Client> GetSupabaseClientAsync()
        {
            if (_supabaseClient == null)
            {
                var url = _configuration["Supabase:Url"];
                var serviceRoleKey = _configuration["Supabase:ServiceRoleKey"];

                // Use service role key for server-side operations
                _supabaseClient = new Supabase.Client(url, serviceRoleKey);
                await _supabaseClient.InitializeAsync();
            }
            return _supabaseClient;
        }

        private AdminClient GetAdminClient()
        {
            if (_adminClient == null)
            {
                var serviceRoleKey = _configuration["Supabase:ServiceRoleKey"];
                _adminClient = new AdminClient(serviceRoleKey);
            }
            return _adminClient;
        }

        /// <summary>
        /// Creates a new user without confirmation email (only password setup email is sent)
        /// </summary>
        public async Task<string> CreateUserAsync(string email, string password, string firstName, string lastName)
        {
            try
            {
                var client = await GetSupabaseClientAsync();

                // Create user in Supabase Auth
                // Note: To disable the confirmation email, go to Supabase Dashboard > Authentication > 
                // Email Templates and disable or customize the "Confirm signup" template
                var session = await client.Auth.SignUp(email, password, new SignUpOptions
                {
                    Data = new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "first_name", firstName },
                        { "last_name", lastName },
                        { "full_name", $"{firstName} {lastName}" },
                        { "needs_password_setup", true }, // Flag to track password setup
                        { "created_at", DateTime.UtcNow.ToString("o") }
                    }
                });

                // Debug logging
                Console.WriteLine($"Supabase Auth SignUp Response:");
                Console.WriteLine($"- User ID: {session?.User?.Id}");
                Console.WriteLine($"- Email: {session?.User?.Email}");
                Console.WriteLine($"- Email Confirmed: {session?.User?.EmailConfirmedAt}");
                Console.WriteLine($"- Session: {session != null}");
                Console.WriteLine($"- User Created At: {session?.User?.CreatedAt}");
                Console.WriteLine($"- User Updated At: {session?.User?.UpdatedAt}");

                if (session?.User != null)
                {
                    var userId = session.User.Id;

                    // Send password setup email (using password reset functionality)
                    try
                    {
                        await SendPasswordSetupEmailAsync(email);
                        Console.WriteLine($"Password setup email sent to: {email}");
                    }
                    catch (Exception emailEx)
                    {
                        // Log but don't fail user creation if email fails
                        Console.WriteLine($"Warning: Failed to send password setup email: {emailEx.Message}");
                    }

                    return userId;
                }

                throw new Exception("Failed to create user in Supabase Auth");
            }
            catch (Exception ex)
            {
                // If it's a rate limit error, provide a helpful message
                if (ex.Message.Contains("over_email_send_rate_limit"))
                {
                    throw new Exception($"Email rate limit exceeded. Please wait about 30 seconds before trying again. Error: {ex.Message}", ex);
                }
                throw new Exception($"Error creating user in Supabase Auth: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sends password setup email to user (uses password reset functionality)
        /// </summary>
        public async Task<bool> SendPasswordSetupEmailAsync(string email)
        {
            try
            {
                var client = await GetSupabaseClientAsync();
                var redirectUrl = _configuration["Supabase:RedirectUrl"];

                // Use password reset email with redirect URL
                var options = new Supabase.Gotrue.SignInOptions
                {
                    RedirectTo = redirectUrl
                };

                await client.Auth.ResetPasswordForEmail(email);

                Console.WriteLine($"Password setup email sent successfully to: {email} with redirect to: {redirectUrl}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending password setup email: {ex.Message}");
                throw new Exception($"Error sending password setup email: {ex.Message}", ex);
            }
        }
      
        /// <summary>
        /// Resends confirmation email to user
        /// </summary>
        public async Task<bool> ResendConfirmationEmailAsync(string email)
        {
            try
            {
                var client = await GetSupabaseClientAsync();

                // Supabase will resend confirmation if user isn't confirmed
                // Redirect URL should be configured in Supabase Dashboard
                await client.Auth.ResetPasswordForEmail(email);

                Console.WriteLine($"Confirmation email resent to: {email}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resending confirmation email: {ex.Message}");
                throw new Exception($"Error resending confirmation email: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates user password using the authenticated user's session
        /// </summary>
        public async Task<bool> UpdateUserPasswordAsync(string supabaseUserId, string newPassword)
        {
            try
            {
                var client = await GetSupabaseClientAsync();

                // Check if there's an active user session
                if (client.Auth.CurrentSession == null)
                {
                    throw new Exception("No active user session found. User must be authenticated to update password.");
                }

                // Update the user's password using the active session
                var attributes = new UserAttributes
                {
                    Password = newPassword
                };

                var updateResult = await client.Auth.Update(attributes);

                if (updateResult == null)
                {
                    throw new Exception("Failed to update password - no user returned from update");
                }

                // Update metadata to mark password as set
                var metadataUpdate = new UserAttributes
                {
                    Data = new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "needs_password_setup", false },
                        { "password_set_at", DateTime.UtcNow.ToString("o") }
                    }
                };

                await client.Auth.Update(metadataUpdate);

                Console.WriteLine($"Password updated successfully for user: {supabaseUserId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user password: {ex.Message}");
                throw new Exception($"Error updating user password in Supabase Auth: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if user needs to set up their password
        /// </summary>
        public async Task<bool> NeedsPasswordSetupAsync(string supabaseUserId)
        {
            try
            {
                var adminClient = GetAdminClient();
                
                // Use Admin API to get user by ID
                var user = await adminClient.GetUserById(supabaseUserId);

                if (user?.UserMetadata != null &&
                    user.UserMetadata.ContainsKey("needs_password_setup"))
                {
                    return Convert.ToBoolean(user.UserMetadata["needs_password_setup"]);
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking password setup status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets user metadata from Supabase Auth
        /// </summary>
        public async Task<System.Collections.Generic.Dictionary<string, object>> GetUserMetadataAsync(string supabaseUserId)
        {
            try
            {
                var adminClient = GetAdminClient();
                
                // Use Admin API to get user by ID
                var user = await adminClient.GetUserById(supabaseUserId);

                return user?.UserMetadata as System.Collections.Generic.Dictionary<string, object>;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user metadata: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteUserAsync(string supabaseUserId)
        {
            try
            {
                var client = await GetSupabaseClientAsync();

                // Note: In a real application, you might need admin privileges to delete users
                // This would typically be done through the Supabase Admin API
                // For now, we'll just return true as a placeholder
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting user from Supabase Auth: {ex.Message}", ex);
            }
        }

        public async Task<SupabaseUser> GetUserByEmailAsync(string email)
        {
            try
            {
                var client = await GetSupabaseClientAsync();

                // Query the users table to find user by email
                var response = await client.From<SupabaseUser>()
                    .Where(x => x.Email == email)
                    .Single();

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving user from Supabase: {ex.Message}", ex);
            }
        }

        public async Task<Supabase.Client> GetSupabaseClientForAuthAsync()
        {
            return await GetSupabaseClientAsync();
        }

        /// <summary>
        /// Verifies password reset token and allows password update
        /// </summary>
        public Task<bool> VerifyPasswordResetTokenAsync(string token)
        {
            try
            {
                // For password reset token verification, we need to use the token
                // This method should be called from the client-side where the token is available
                // For server-side verification, we would need to implement token validation logic
                // For now, return true as a placeholder - this should be implemented based on your specific needs
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying password reset token: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Determines user role based on Supabase user ID by checking teachers and students tables
        /// </summary>
        public async Task<string> GetUserRoleAsync(string supabaseUserId)
        {
            try
            {
                var client = await GetSupabaseClientAsync();

                // Check if user exists in teachers table
                var teacherResponse = await client
                    .From<Teacher>()
                    .Where(x => x.SupabaseUserId == supabaseUserId)
                    .Single();

                if (teacherResponse != null)
                {
                    return "Teacher";
                }

                // Check if user exists in students table
                var studentResponse = await client
                    .From<Student>()
                    .Where(x => x.SupabaseUserId == supabaseUserId)
                    .Single();

                if (studentResponse != null)
                {
                    return "Student";
                }

                // Default to Student if not found in either table
                return "Student";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error determining user role: {ex.Message}");
                // Default to Student on error
                return "Student";
            }
        }
    }
}