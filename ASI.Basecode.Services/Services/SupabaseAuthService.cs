using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private Supabase.Client _supabaseClient;
        private Supabase.Gotrue.Client _gotrueClient;
        private AdminClient _adminClient;

        public SupabaseAuthService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the redirect URL dynamically based on current request or falls back to config
        /// </summary>
        private string GetRedirectUrl()
        {
            try
            {
                // Check if we should use dynamic local redirect (for development)
                var useLocalRedirect = _configuration.GetValue<bool>("Supabase:UseLocalRedirect", false);

                if (useLocalRedirect && _httpContextAccessor.HttpContext != null)
                {
                    var request = _httpContextAccessor.HttpContext.Request;
                    var scheme = request.Scheme; // http or https
                    var host = request.Host.ToString(); // localhost:port
                    var redirectUrl = $"{scheme}://{host}/Account/SetPassword";

                    Console.WriteLine($"🔧 Dynamic Redirect URL: {redirectUrl}");
                    return redirectUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Warning: Could not determine dynamic redirect URL: {ex.Message}");
            }

            // Fall back to configured redirect URL
            var configuredUrl = _configuration["Supabase:RedirectUrl"];
            Console.WriteLine($"📝 Using Configured Redirect URL: {configuredUrl}");
            return configuredUrl;
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
                Console.WriteLine($"=== SendPasswordSetupEmailAsync ===");
                Console.WriteLine($"Target email: {email}");

                var gotrueClient = GetGotrueClient();
                var redirectUrl = GetRedirectUrl();

                Console.WriteLine($"Redirect URL: {redirectUrl}");
                Console.WriteLine($"⚠ IMPORTANT: This redirect URL must be added to Supabase Dashboard:");
                Console.WriteLine($"   Navigate to: Authentication > URL Configuration > Redirect URLs");
                Console.WriteLine($"   Add URL: {redirectUrl}");
                Console.WriteLine($"   Recommended: Add wildcard http://localhost:*/Account/SetPassword for all dev ports");
                Console.WriteLine($"Sending password reset email via Gotrue...");

                // ✅ Note: Redirect URL must be configured in Supabase Dashboard
                // The current version of Supabase.Gotrue library doesn't support passing options
                // Supabase will use the Site URL configured in dashboard
                await gotrueClient.ResetPasswordForEmail(email);

                Console.WriteLine($"✓ Password setup email sent successfully to: {email}");
                Console.WriteLine($"=== End SendPasswordSetupEmailAsync ===\n");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error sending password setup email: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine($"=== End SendPasswordSetupEmailAsync (FAILED) ===\n");
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
        /// Determines user role based on Supabase user ID by checking the users, user_roles, and roles tables
        /// </summary>
        public async Task<string> GetUserRoleAsync(string supabaseUserId)
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
                        return "Student"; // Default to Student if not in database
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Error querying users table: {ex.Message}");
                    return "Student";
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
                        return "Student"; // Default to Student if no role assigned
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Error querying user_roles table: {ex.Message}");
                    return "Student";
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
                        return roleRecord.RoleName;
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
                return "Student";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ FATAL ERROR in GetUserRoleAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                // Return Student as safe default instead of throwing
                return "Student";
            }
        }

        /// <summary>
        /// Gets user role and name by Supabase user ID
        /// </summary>
      public async Task<(string Role, string Name)> GetUserRoleAndNameAsync(string supabaseUserId)
        {
     try
            {
      Console.WriteLine($"\n=== LOADING USER ROLE AND NAME ===");
      Console.WriteLine($"Supabase User ID: {supabaseUserId}");

         var client = await GetSupabaseClientAsync();

      // Step 1: Get user record from users table
       Console.WriteLine($"Step 1: Querying users table...");
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
      Console.WriteLine($"✓ Found user record in users table");
              Console.WriteLine($"  - User ID (DB): {userRecord.Id}");
             Console.WriteLine($"  - First Name: {userRecord.FirstName}");
     Console.WriteLine($"  - Last Name: {userRecord.LastName}");
       }
     else
         {
        Console.WriteLine($"✗ No user found in users table with userTypeId: {supabaseUserId}");
              return ("Student", "User"); // Default if not in database
      }
       }
catch (Exception ex)
                {
     Console.WriteLine($"✗ Error querying users table: {ex.Message}");
         return ("Student", "User");
          }

            // Step 2: Get the user's role
 var roleName = await GetUserRoleAsync(supabaseUserId);
                
   // Step 3: Build full name
       var fullName = $"{userRecord.FirstName} {userRecord.LastName}".Trim();
         if (string.IsNullOrWhiteSpace(fullName))
   {
           fullName = userRecord.Email?.Split('@')[0] ?? "User";
         }

                Console.WriteLine($"\n✓ Role and Name lookup complete:");
                Console.WriteLine($"  - Role: {roleName}");
          Console.WriteLine($"  - Name: {fullName}");
     Console.WriteLine($"=== ROLE AND NAME LOOKUP SUCCESS ===\n");

  return (roleName, fullName);
       }
            catch (Exception ex)
     {
  Console.WriteLine($"✗ FATAL ERROR in GetUserRoleAndNameAsync: {ex.Message}");
       Console.WriteLine($"Stack Trace: {ex.StackTrace}");
         // Return defaults as safe fallback
         return ("Student", "User");
            }
 }

        /// <summary>
   /// Signs in a user with email and password using ANON key (public authentication)
    /// </summary>
        public async Task<Supabase.Gotrue.Session> SignInAsync(string email, string password)
   {
          try
       {
    Console.WriteLine($"=== SIGN IN ATTEMPT ===");
 Console.WriteLine($"Email: {email}");
         Console.WriteLine($"Password Length: {password?.Length ?? 0} characters");

        // ✅ Use ANON KEY for user sign-in (not Service Role Key)
   var url = _configuration["Supabase:Url"];
       var anonKey = _configuration["Supabase:AnonKey"];

     Console.WriteLine($"Using Anon Key for authentication: {anonKey?.Substring(0, Math.Min(20, anonKey?.Length ?? 0))}...");
         Console.WriteLine($"Supabase URL: {url}");

      // Create a separate client with ANON key for authentication
      var authOptions = new SupabaseOptions
     {
       AutoConnectRealtime = false,
         AutoRefreshToken = true,
           Headers = new System.Collections.Generic.Dictionary<string, string>
      {
    { "X-Client-Info", "supabase-csharp/1.1.1" }
           }
    };

          var authClient = new Supabase.Client(url, anonKey, authOptions);
     await authClient.InitializeAsync();

    Console.WriteLine("✓ Auth client initialized with Anon Key");
     Console.WriteLine($"Attempting to sign in with Supabase Auth...");

      try
           {
          // Sign in using the auth client with anon key
         var session = await authClient.Auth.SignIn(email, password);

            if (session != null)
           {
     Console.WriteLine($"✓ Sign in SUCCESS!");
          Console.WriteLine($"  - User ID: {session.User?.Id}");
      Console.WriteLine($"  - Email: {session.User?.Email}");
     Console.WriteLine($"  - Email Confirmed: {session.User?.EmailConfirmedAt.HasValue}");
           Console.WriteLine($"  - Email Confirmed At: {session.User?.EmailConfirmedAt}");
   }
                 else
         {
            Console.WriteLine($"✗ Sign in returned NULL session");
    Console.WriteLine($"  → This usually means authentication failed");
            }

       return session;
                }
    catch (Supabase.Gotrue.Exceptions.GotrueException gex)
  {
        Console.WriteLine($"✗ GOTRUE EXCEPTION during sign in:");
    Console.WriteLine($"  - Message: {gex.Message}");
        Console.WriteLine($"  - Status Code: {gex.StatusCode}");
       Console.WriteLine($"  - Content: {gex.Content}");
        
         // ✅ Enhanced error analysis
  if (gex.Message.Contains("Invalid login credentials") || gex.Message.Contains("invalid_credentials"))
              {
              Console.WriteLine($"  → Invalid email or password");
      Console.WriteLine($"");
 Console.WriteLine($"  🔍 TROUBLESHOOTING STEPS:");
    Console.WriteLine($"     1. Check if user exists in Supabase Dashboard:");
        Console.WriteLine($"        → Go to: {url.Replace("https://", "https://app.supabase.com/project/")}/auth/users");
  Console.WriteLine($"        → Search for: {email}");
       Console.WriteLine($"     2. If user NOT found:");
            Console.WriteLine($"    → Create user manually in Dashboard");
      Console.WriteLine($"        → OR use registration system");
    Console.WriteLine($"     3. If user found but login fails:");
       Console.WriteLine($"        → Check 'Email Confirmed' column has a timestamp");
           Console.WriteLine($"→ Try resetting password in Dashboard");
      Console.WriteLine($"     → Verify email case matches (should be lowercase)");
      }
   else if (gex.Message.Contains("Email not confirmed"))
       {
          Console.WriteLine($"  → User email not verified");
            Console.WriteLine($"  → ACTION: Confirm email in Supabase Dashboard or resend verification email");
      }
             else if (gex.Message.Contains("rate limit") || gex.Message.Contains("too many"))
        {
          Console.WriteLine($"  → Too many attempts - please wait a few minutes");
          }
         
         throw; // Re-throw to be caught by AuthController
         }
            }
  catch (Supabase.Gotrue.Exceptions.GotrueException)
  {
    throw; // Already logged above
         }
   catch (Exception ex)
    {
             Console.WriteLine($"✗ UNEXPECTED EXCEPTION during sign in:");
                Console.WriteLine($"  - Type: {ex.GetType().Name}");
   Console.WriteLine($"  - Message: {ex.Message}");
         Console.WriteLine($"  - Stack Trace: {ex.StackTrace}");
                throw;
         }
        }
    }
}