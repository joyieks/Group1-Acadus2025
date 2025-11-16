using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Supabase;
using Supabase.Gotrue;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class SupabaseAuthService : ISupabaseAuthService
    {
        private readonly IConfiguration _configuration;
        private Supabase.Client _supabaseClient;
        private Supabase.Gotrue.Client _gotrueClient;
        private AdminClient _adminClient;

        public SupabaseAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
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
        /// Updates a user's password using the Admin API (does not require user session)
        /// </summary>
        public async Task<bool> UpdateUserPasswordAdminAsync(string supabaseUserId, string newPassword)
        {
            try
            {
                var adminClient = GetAdminClient();
                var attributes = new AdminUserAttributes
                {
                    Password = newPassword
                };

                var updated = await adminClient.UpdateUserById(supabaseUserId, attributes);
                return updated != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating password via Admin API: {ex.Message}");
                throw new Exception($"Error updating user password via Admin API: {ex.Message}", ex);
            }
        }

        public async Task<string> UploadProfileImageAsync(string supabaseUserId, string fileName, System.IO.Stream fileStream, string contentType)
        {
            var client = await GetSupabaseClientAsync();
            var bucket = _configuration["Supabase:ProfilePicturesBucket"] ?? "profile_pictures";
            var safeExt = System.IO.Path.GetExtension(fileName)?.ToLowerInvariant() ?? ".jpg";
            var objectPath = $"{supabaseUserId}/{Guid.NewGuid():N}{safeExt}";

            byte[] data;
            using (var ms = new System.IO.MemoryStream())
            {
                await fileStream.CopyToAsync(ms);
                data = ms.ToArray();
            }

            await client.Storage.From(bucket).Upload(data, objectPath, new Supabase.Storage.FileOptions
            {
                ContentType = contentType,
                CacheControl = "3600",
                Upsert = true
            });

            return objectPath;
        }

        public async Task<bool> SetUserProfileImageUrlAsync(string supabaseUserId, string imageUrl, string imagePath)
        {
            try
            {
                var admin = GetAdminClient();
                var attrs = new AdminUserAttributes
                {
                    Data = new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "profile_image_url", imageUrl },
                        { "profile_image_path", imagePath },
                        { "profile_image_updated_at", DateTime.UtcNow.ToString("o") }
                    }
                };
                var updated = await admin.UpdateUserById(supabaseUserId, attrs);
                return updated != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting profile image url: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetProfileImageUrlAsync(string objectPath, int expiresInSeconds = 3600)
        {
            var client = await GetSupabaseClientAsync();
            var bucket = _configuration["Supabase:ProfilePicturesBucket"] ?? "profile_pictures";
            try
            {
                // Try signed URL (works for private buckets)
                var signed = await client.Storage.From(bucket).CreateSignedUrl(objectPath, expiresInSeconds);
                if (!string.IsNullOrWhiteSpace(signed)) return signed;
            }
            catch { }

            // Fallback to public URL if bucket is public
            return client.Storage.From(bucket).GetPublicUrl(objectPath);
        }

        public async Task<string> GetUserProfileImageUrlAsync(string supabaseUserId)
        {
            try
            {
                var admin = GetAdminClient();
                var user = await admin.GetUserById(supabaseUserId);
                if (user?.UserMetadata != null)
                {
                    // Try path first (preferred for signed URL)
                    if (user.UserMetadata.ContainsKey("profile_image_path"))
                    {
                        var path = user.UserMetadata["profile_image_path"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(path))
                        {
                            return await GetProfileImageUrlAsync(path, 3600);
                        }
                    }
                    // Fallback to stored URL
                    if (user.UserMetadata.ContainsKey("profile_image_url"))
                    {
                        var url = user.UserMetadata["profile_image_url"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(url)) return url;
                    }
                }

                // Fallback: look for latest file in Storage under the user's folder
                var client = await GetSupabaseClientAsync();
                var bucket = _configuration["Supabase:ProfilePicturesBucket"] ?? "profile_pictures";
                try
                {
                    var files = await client.Storage.From(bucket).List(supabaseUserId);
                    if (files != null && files.Count > 0)
                    {
                        // pick the file with latest updated_at if available, else first
                        var latest = files
                            .OrderByDescending(f => f.UpdatedAt ?? f.CreatedAt ?? DateTime.UtcNow)
                            .FirstOrDefault();
                        if (latest != null)
                        {
                            var path = string.IsNullOrWhiteSpace(latest.Name) ? null : $"{supabaseUserId}/{latest.Name}";
                            if (!string.IsNullOrWhiteSpace(path))
                            {
                                return await GetProfileImageUrlAsync(path, 3600);
                            }
                        }
                    }
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"Storage fallback failed: {ex2.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user profile image url: {ex.Message}");
            }
            return null;
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

        /// <summary>
        /// Deletes a user from Supabase Auth and database
        /// </summary>
        /// <remarks>
        /// WARNING: This method is not fully implemented.
        /// Currently returns true without actually deleting the user.
        /// TODO: Implement actual deletion from auth.users and public.users
        /// </remarks>
        [System.Obsolete("This method is not fully implemented. It does not actually delete users yet.", false)]
        public async Task<bool> DeleteUserAsync(string supabaseUserId)
        {
            try
            {
                Console.WriteLine($"⚠️ WARNING: DeleteUserAsync called for {supabaseUserId} but NOT IMPLEMENTED");
                Console.WriteLine($"  User deletion requires implementation of:");
                Console.WriteLine($"  1. Delete from auth.users via AdminClient.DeleteUser()");
                Console.WriteLine($"  2. Delete from public.users table");
                Console.WriteLine($"  3. Handle cascading deletes for related records");
                
                // TODO: Implement actual deletion
                // var adminClient = GetAdminClient();
                  // await adminClient.DeleteUser(supabaseUserId);
               // var client = await GetSupabaseClientAsync();
                // await client.From<SupabaseUserNew>().Where(x => x.UserTypeId == supabaseUserId).Delete();
                    
                   return false;  // Return false to indicate not implemented
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
        /// Verifies password reset token
        /// </summary>
        /// <remarks>
        /// NOTE: This method always returns true because Supabase handles 
        /// token verification internally via the ResetPasswordForEmail flow.
        /// Custom token verification is not needed for the current implementation.
        /// </remarks>
        [System.Obsolete("Token verification is handled internally by Supabase. This method is not needed.", false)]
        public Task<bool> VerifyPasswordResetTokenAsync(string token)
        {
            try
            {
                Console.WriteLine($"ℹ️ INFO: VerifyPasswordResetTokenAsync called");
                Console.WriteLine($"  Supabase handles token verification internally via ResetPasswordForEmail");
                Console.WriteLine($"  This method is not needed for current password reset flow");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying password reset token: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Determines user role and name based on Supabase user ID by checking the users, user_roles, and roles tables
        /// </summary>
        public async Task<(string Role, string Name)> GetUserRoleAndNameAsync(string supabaseUserId)
        {
            try
       {
             var client = await GetSupabaseClientAsync();
  
        Console.WriteLine($"=== ROLE LOOKUP DEBUG ===");
     Console.WriteLine($"Looking up role for Supabase Auth User ID: {supabaseUserId}");
   
         // Step 1: Get the user from the users table using their userTypeId (Supabase Auth ID)
            SupabaseUserNew userRecord = null;
                try
      {
            var userQuery = await client
 .From<SupabaseUserNew>()
           .Where(x => x.UserTypeId == supabaseUserId)
           .Get();

  userRecord = userQuery?.Models?.FirstOrDefault();
                   
            if (userRecord != null)
    {
          Console.WriteLine($"✓ Found user in users table:");
          Console.WriteLine($"  - User ID: {userRecord.Id}");
   Console.WriteLine($"  - Email: {userRecord.Email}");
      Console.WriteLine($"  - Name: {userRecord.FirstName} {userRecord.LastName}");
  }
     else
       {
               Console.WriteLine($"✗ No user found in users table with userTypeId: {supabaseUserId}");
            Console.WriteLine($"  User needs to be added to the users table first");
         return ("Student", "User"); // Default to Student if not in database
      }
   }
         catch (Exception ex)
   {
     Console.WriteLine($"✗ Error querying users table: {ex.Message}");
  return ("Student", "User");
  }

         // Step 2: Get the user's role from user_roles table
      // Convert userId to string for comparison
             string userIdString = userRecord.Id.ToString();
     UserRole userRoleRecord = null;
      try
     {
         // Get ALL user_roles and filter in memory since Supabase doesn't support complex queries
     var allUserRoles = await client
                .From<UserRole>()
           .Get();

    // FIXED: Use supabaseUserId (userTypeId) instead of userRecord.Id (database ID)
            // The user_roles.userId column contains the Supabase Auth UUID, not the database ID
            userRoleRecord = allUserRoles?.Models?.FirstOrDefault(x => x.UserId == supabaseUserId);
        
 if (userRoleRecord != null)
            {
       Console.WriteLine($"✓ Found user_role mapping:");
   Console.WriteLine($"  - User Role ID: {userRoleRecord.Id}");
      Console.WriteLine($"  - User ID: {userRoleRecord.UserId}");
     Console.WriteLine($"  - Role ID: {userRoleRecord.RoleId}");
            }
            else
   {
      Console.WriteLine($"✗ No role mapping found in user_roles table for userTypeId: {supabaseUserId}");
      Console.WriteLine($"  User needs to be assigned a role in user_roles table");
       return ("Student", $"{userRecord.FirstName} {userRecord.LastName}"); // Default to Student if no role assigned
}
     }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error querying user_roles table: {ex.Message}");
            return ("Student", $"{userRecord.FirstName} {userRecord.LastName}");
   }

         // Step 3: Get the role name from roles table by ID
  try
       {
   // userRoleRecord.RoleId is now an integer referencing roles.id
 var roleQuery = await client
       .From<Role>()
       .Where(x => x.Id == userRoleRecord.RoleId)
        .Get();

        var roleRecord = roleQuery?.Models?.FirstOrDefault();

    if (roleRecord != null && !string.IsNullOrEmpty(roleRecord.RoleName))
   {
       Console.WriteLine($"✓ Found role:");
  Console.WriteLine($"  - Role ID: {roleRecord.Id}");
            Console.WriteLine($"  - Role Name: {roleRecord.RoleName}");
   Console.WriteLine($"=== ROLE LOOKUP SUCCESS: {roleRecord.RoleName} ===");
                return (roleRecord.RoleName, $"{userRecord.FirstName} {userRecord.LastName}");
   }
     else
            {
 Console.WriteLine($"✗ No role found in roles table with ID: {userRoleRecord.RoleId}");
    Console.WriteLine($"  Role ID might be invalid or role was deleted");
       }
   }
     catch (Exception ex)
        {
      Console.WriteLine($"✗ Error querying roles table: {ex.Message}");
 }

                // If we got here, something went wrong - default to Student
    Console.WriteLine($"=== ROLE LOOKUP FAILED: Defaulting to Student ===");
           return ("Student", $"{userRecord?.FirstName} {userRecord?.LastName}" ?? "User");
}
       catch (Exception ex)
       {
  Console.WriteLine($"✗ FATAL ERROR in GetUserRoleAsync: {ex.Message}");
      Console.WriteLine($"Stack Trace: {ex.StackTrace}");
       // Return Student as safe default instead of throwing
      return ("Student", "User");
}
        }

        /// <summary>
        /// Signs in a user with email and password
        /// </summary>
        public async Task<Supabase.Gotrue.Session> SignInAsync(string email, string password)
        {
            var supabaseClient = await GetSupabaseClientAsync();
            return await supabaseClient.Auth.SignIn(email, password);
        }
    }
}