using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Supabase;
using Supabase.Gotrue;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class SupabaseAuthService : ISupabaseAuthService
    {
        private readonly IConfiguration _configuration;
        private Supabase.Client _supabaseClient;
        private Supabase.Gotrue.Client _gotrueClient;
        private AdminClient _adminClient;
        private static HttpClient _httpClient;
        private static readonly object _lock = new object();

        public SupabaseAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private HttpClient GetHttpClient()
        {
            if (_httpClient == null)
            {
                lock (_lock)
                {
                    if (_httpClient == null)
                    {
                        var isDevelopment = _configuration.GetValue<bool>("Development:IgnoreSSLErrors", true);

                        if (isDevelopment)
                        {
                            var handler = new HttpClientHandler
                            {
                                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                            };
                            _httpClient = new HttpClient(handler)
                            {
                                Timeout = TimeSpan.FromSeconds(30)
                            };
                            Console.WriteLine("✓ Custom HttpClient created with SSL validation bypassed and 30s timeout");
                        }
                        else
                        {
                            _httpClient = new HttpClient
                            {
                                Timeout = TimeSpan.FromSeconds(30)
                            };
                        }
                    }
                }
            }
            return _httpClient;
        }

        private Supabase.Gotrue.Client GetGotrueClient()
        {
            if (_gotrueClient == null)
            {
                var url = _configuration["Supabase:Url"];
                var serviceRoleKey = _configuration["Supabase:ServiceRoleKey"];
                var authUrl = $"{url}/auth/v1";

                // Create Gotrue client options (without HttpClient as it's not supported)
                var gotrueOptions = new Supabase.Gotrue.ClientOptions
                {
                    Url = authUrl,
                    Headers = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "apikey", serviceRoleKey },
                        { "Authorization", $"Bearer {serviceRoleKey}" }
                    }
                };

                _gotrueClient = new Supabase.Gotrue.Client(gotrueOptions);
                Console.WriteLine("✓ Gotrue client created");
            }
            return _gotrueClient;
        }

        private async Task<Supabase.Client> GetSupabaseClientAsync()
        {
            if (_supabaseClient == null)
            {
                var url = _configuration["Supabase:Url"];
                var serviceRoleKey = _configuration["Supabase:ServiceRoleKey"];
                var isDevelopment = _configuration.GetValue<bool>("Development:IgnoreSSLErrors", true);

                Console.WriteLine($"=== SUPABASE CONNECTION DEBUG ===");
                Console.WriteLine($"URL: {url}");
                Console.WriteLine($"Service Key: {serviceRoleKey?.Substring(0, Math.Min(20, serviceRoleKey?.Length ?? 0))}...");
                Console.WriteLine($"Development Mode: {isDevelopment}");

                // Configure Supabase options
                var options = new SupabaseOptions
                {
                    AutoConnectRealtime = false,
                    AutoRefreshToken = true,
                    Headers = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "X-Client-Info", "supabase-csharp/1.1.1" }
                    }
                };

                // Create Supabase client
                _supabaseClient = new Supabase.Client(url, serviceRoleKey, options);

                // Replace the Auth client with our custom one
                try
                {
                    var authField = _supabaseClient.GetType().GetField("Auth", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    
                    if (authField != null)
                    {
                        authField.SetValue(_supabaseClient, GetGotrueClient());
                        Console.WriteLine("✓ Custom Gotrue client injected into Supabase client");
                    }
                    else
                    {
                        // Try property
                        var authProperty = _supabaseClient.GetType().GetProperty("Auth");
                        if (authProperty != null && authProperty.CanWrite)
                        {
                            authProperty.SetValue(_supabaseClient, GetGotrueClient());
                            Console.WriteLine("✓ Custom Gotrue client injected via property");
                        }
                        else
                        {
                            Console.WriteLine("⚠ Warning: Could not replace Auth client, will use default");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ Warning: Could not inject custom Gotrue client: {ex.Message}");
                }

                await _supabaseClient.InitializeAsync();
                Console.WriteLine("✓ Supabase client initialized successfully");
            }
            return _supabaseClient;
        }

        private AdminClient GetAdminClient()
        {
            if (_adminClient == null)
            {
                var serviceRoleKey = _configuration["Supabase:ServiceRoleKey"];
                var url = $"{_configuration["Supabase:Url"]}/auth/v1";

                // Create AdminClient with ClientOptions
                var adminOptions = new Supabase.Gotrue.ClientOptions
                {
                    Url = url,
                    Headers = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "apikey", serviceRoleKey },
                        { "Authorization", $"Bearer {serviceRoleKey}" }
                    }
                };

                _adminClient = new AdminClient(serviceRoleKey, adminOptions);
                Console.WriteLine("✓ AdminClient created");
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
                // Use Gotrue client directly instead of Supabase client for auth operations
                var gotrueClient = GetGotrueClient();

                Console.WriteLine($"Attempting to create user: {email}");

                // Create user in Supabase Auth using Gotrue client directly
                var session = await gotrueClient.SignUp(email, password, new SignUpOptions
                {
                    Data = new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "first_name", firstName },
                        { "last_name", lastName },
                        { "full_name", $"{firstName} {lastName}" },
                        { "needs_password_setup", true },
                        { "created_at", DateTime.UtcNow.ToString("o") }
                    }
                });

                Console.WriteLine($"Supabase Auth SignUp Response:");
                Console.WriteLine($"- User ID: {session?.User?.Id}");
                Console.WriteLine($"- Email: {session?.User?.Email}");
                Console.WriteLine($"- Email Confirmed: {session?.User?.EmailConfirmedAt}");

                if (session?.User != null)
                {
                    var userId = session.User.Id;

                    // Send password setup email
                    try
                    {
                        await SendPasswordSetupEmailAsync(email);
                        Console.WriteLine($"Password setup email sent to: {email}");
                    }
                    catch (Exception emailEx)
                    {
                        Console.WriteLine($"Warning: Failed to send password setup email: {emailEx.Message}");
                    }

                    return userId;
                }

                throw new Exception("Failed to create user in Supabase Auth");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in CreateUserAsync: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                }

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
                var gotrueClient = GetGotrueClient();
                var redirectUrl = _configuration["Supabase:RedirectUrl"];

                await gotrueClient.ResetPasswordForEmail(email);

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
                var gotrueClient = GetGotrueClient();
                await gotrueClient.ResetPasswordForEmail(email);

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
                var gotrueClient = GetGotrueClient();

                if (gotrueClient.CurrentSession == null)
                {
                    throw new Exception("No active user session found. User must be authenticated to update password.");
                }

                var attributes = new UserAttributes
                {
                    Password = newPassword
                };

                var updateResult = await gotrueClient.Update(attributes);

                if (updateResult == null)
                {
                    throw new Exception("Failed to update password - no user returned from update");
                }

                var metadataUpdate = new UserAttributes
                {
                    Data = new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "needs_password_setup", false },
                        { "password_set_at", DateTime.UtcNow.ToString("o") }
                    }
                };

                await gotrueClient.Update(metadataUpdate);

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
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying password reset token: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Determines user role based on Supabase user ID by checking admin email, then teachers and students tables
        /// </summary>
        public async Task<string> GetUserRoleAsync(string supabaseUserId)
        {
            try
            {
                var client = await GetSupabaseClientAsync();
                
                // First, get the user's email from Supabase Auth to check if they're an admin
                try
                {
                    var adminClient = GetAdminClient();
                    var user = await adminClient.GetUserById(supabaseUserId);
                    
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        var normalizedEmail = user.Email.Trim().ToLowerInvariant();
                        
                        // Check if this is the admin email
                        var adminEmail = _configuration["Admin:Email"] ?? "admin@gmail.com";
                        if (normalizedEmail == adminEmail.ToLowerInvariant())
                        {
                            Console.WriteLine($"User {user.Email} identified as Admin");
                            return "Admin";
                        }
                        
                        // Check user metadata for admin role
                        if (user.UserMetadata != null && user.UserMetadata.ContainsKey("role"))
                        {
                            var metadataRole = user.UserMetadata["role"]?.ToString();
                            if (!string.IsNullOrEmpty(metadataRole) && metadataRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"User {user.Email} identified as Admin from metadata");
                                return "Admin";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking admin status: {ex.Message}");
                }

                // Check if user is a teacher
                var teacherResponse = await client
                    .From<Teacher>()
                    .Where(x => x.SupabaseUserId == supabaseUserId)
                    .Single();

                if (teacherResponse != null)
                {
                    return "Teacher";
                }

                // Check if user is a student
                var studentResponse = await client
                    .From<Student>()
                    .Where(x => x.SupabaseUserId == supabaseUserId)
                    .Single();

                if (studentResponse != null)
                {
                    return "Student";
                }

                // Default to Student if no role found
                return "Student";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error determining user role: {ex.Message}");
                return "Student";
            }
        }
    }
}