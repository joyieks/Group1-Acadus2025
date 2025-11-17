using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.Extensions.Configuration;
using Supabase;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IConfiguration _configuration;
        private readonly IdGeneratorService _idGenerator;
        private Supabase.Client _supabaseClient;
        private static HttpClient _httpClient; // ? NEW

        public TeacherService(
            ISupabaseAuthService supabaseAuthService, 
            IConfiguration configuration,
            IdGeneratorService idGenerator)
        {
            _supabaseAuthService = supabaseAuthService;
            _configuration = configuration;
            _idGenerator = idGenerator;
        }

        // ? NEW: Add HttpClient with SSL bypass for development
        private HttpClient GetHttpClient()
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
                    _httpClient = new HttpClient(handler);
                    Console.WriteLine("[TeacherService] ? Custom HttpClient created with SSL validation bypassed");
                }
                else
                {
                    _httpClient = new HttpClient();
                }
            }
            return _httpClient;
        }

        private async Task<Supabase.Client> GetSupabaseClientAsync()
        {
            if (_supabaseClient == null)
            {
                var url = _configuration["Supabase:Url"];
                var serviceRoleKey = _configuration["Supabase:ServiceRoleKey"];
                var isDevelopment = _configuration.GetValue<bool>("Development:IgnoreSSLErrors", true);

                Console.WriteLine($"[TeacherService] Initializing Supabase Client");
                Console.WriteLine($"  URL: {url}");
                Console.WriteLine($"  Development Mode: {isDevelopment}");

                var options = new SupabaseOptions
                {
                    AutoConnectRealtime = false,
                    AutoRefreshToken = true
                };

                _supabaseClient = new Supabase.Client(url, serviceRoleKey, options);

                // ? Inject custom HttpClient using reflection (like StudentService)
                var httpClientProperty = _supabaseClient.GetType().GetProperty("HttpClient");
                if (httpClientProperty != null && isDevelopment)
                {
                    httpClientProperty.SetValue(_supabaseClient, GetHttpClient());
                    Console.WriteLine("  ? Custom HttpClient injected with SSL validation bypassed");
                }

                await _supabaseClient.InitializeAsync();
                Console.WriteLine("[TeacherService] ? Supabase client initialized");
            }
            return _supabaseClient;
        }

        public async Task<bool> CreateTeacherAsync(TeacherViewModel model)
        {
            try
            {
                Console.WriteLine($"\n=== CREATING TEACHER: {model.FirstName} {model.LastName} ===");

                // Generate a secure random password
                var secureRandomPassword = Guid.NewGuid().ToString() + "Aa1!";

                Console.WriteLine($"Step 1: Creating Supabase Auth user...");
                var supabaseUserId = await _supabaseAuthService.CreateUserAsync(
                    model.Email,
                    secureRandomPassword,
                    model.FirstName,
                    model.LastName
                );

                Console.WriteLine($"? Step 1 Complete: Auth user created with ID: {supabaseUserId}");

                var client = await GetSupabaseClientAsync();

                // ? NEW: Step 1.5: Generate unique teacher display ID
                Console.WriteLine($"Step 1.5: Generating unique teacher display ID...");
                var teacherDisplayId = await _idGenerator.GenerateTeacherIdAsync();
                Console.WriteLine($"? Step 1.5 Complete: Generated teacher display ID: {teacherDisplayId}");

                // Step 2: Insert into users table (stores all personal information)
                Console.WriteLine($"Step 2: Inserting into users table...");
                var userRecord = new SupabaseUserNew
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    MiddleName = model.MiddleName,
                    Suffix = model.Suffix,
                    Email = model.Email,
                    ContactNumber = model.ContactNumber,
                    UserTypeId = supabaseUserId, // Supabase Auth UUID
                    UserDisplayId = teacherDisplayId, // ? NEW: Human-readable display ID
                    IsActive = true,
                    ProfilePictureUrl = null,
                    Address = null,
                    EmergencyContact = null
                };

                var insertedUserResponse = await client.From<SupabaseUserNew>().Insert(userRecord);
                var insertedUser = insertedUserResponse.Model;
                Console.WriteLine($"? Step 2 Complete: User record created with ID: {insertedUser.Id} (DisplayId: {teacherDisplayId})");

                // Step 3: Lookup department ID
                Console.WriteLine($"Step 3: Looking up department ID...");
                int? departmentId = null;

                try
                {
                    // Try to find department by name
                    var deptQuery = await client.From<Department>()
                        .Where(x => x.DepartmentName == model.Department)
                        .Get();
                    var deptRecord = deptQuery?.Models?.FirstOrDefault();
                    departmentId = deptRecord?.Id;
                    Console.WriteLine($"  Department lookup: {(departmentId.HasValue ? $"Found ID {departmentId}" : "Not found, will use null")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Warning: Department lookup failed: {ex.Message}");
                }
                Console.WriteLine($"? Step 3 Complete: Department ID = {departmentId}");

                // Step 4: Create teacherProfile record (only stores teacherId and departmentId)
                Console.WriteLine($"Step 4: Creating teacherProfile record...");
                var teacher = new Teacher
                {
                    TeacherId = supabaseUserId,   // References users.userTypeId (UUID)
                    TeacherDisplayId = teacherDisplayId, // ? NEW: Human-readable display ID
                    DepartmentId = departmentId,  // FK to departments table
                    CreatedAt = DateTime.UtcNow
                };

                var insertedTeacherResponse = await client.From<Teacher>().Insert(teacher);
                var insertedTeacher = insertedTeacherResponse.Model;
                Console.WriteLine($"? Step 4 Complete: TeacherProfile created with ID: {insertedTeacher.Id} (DisplayId: {teacherDisplayId})");

                // Step 5: Lookup Teacher role and assign in user_roles table
                Console.WriteLine($"Step 5: Looking up Teacher role and assigning in user_roles table...");

                // Lookup Teacher role by name to get ID
                int teacherRoleId = 2; // Default to 2 if lookup fails
                try
                {
                    var roleQuery = await client.From<Role>()
                    .Where(x => x.RoleName == "Teacher")
                           .Get();
                    var roleRecord = roleQuery?.Models?.FirstOrDefault();
                    if (roleRecord != null)
                    {
                        teacherRoleId = roleRecord.Id;
                        Console.WriteLine($"  Role lookup: Found 'Teacher' role with ID {teacherRoleId}");
                    }
                    else
                    {
                        Console.WriteLine($"  Warning: Teacher role not found, using default ID 2");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Warning: Role lookup failed: {ex.Message}, using default ID 2");
                }

                var userRole = new UserRole
                {
                    UserId = supabaseUserId, // Supabase Auth UUID
                    RoleId = teacherRoleId, // Now an int referencing roles.id
                    CreatedAt = DateTime.UtcNow
                };

                await client.From<UserRole>().Insert(userRole);
                Console.WriteLine($"? Step 5 Complete: Teacher role (ID {teacherRoleId}) assigned");

                // Step 6: Create address record (optional - if addresses are still used)
                if (!string.IsNullOrEmpty(model.City) && !string.IsNullOrEmpty(model.Province))
                {
                    Console.WriteLine($"Step 6: Creating address record...");
                    var address = new Address
                    {
                        HouseNumber = model.HouseNumber,
                        StreetName = model.StreetName,
                        Subdivision = model.Subdivision,
                        Barangay = model.Barangay ?? "N/A",
                        City = model.City,
                        Province = model.Province,
                        ZipCode = model.ZipCode,
                        CreatedAt = DateTime.UtcNow
                    };

                    var insertedAddressResponse = await client.From<Address>().Insert(address);
                    var insertedAddress = insertedAddressResponse.Model;
                    Console.WriteLine($"? Step 6 Complete: Address created with ID: {insertedAddress.Id}");

                    // Update user record with address info (simplified - store as text)
                    var addressText = $"{model.HouseNumber} {model.StreetName}, {model.Barangay}, {model.City}, {model.Province} {model.ZipCode}";
                    Console.WriteLine($"  Address stored: {addressText}");
                }

                // Step 7: Create emergency contact (optional - if contacts are still used)
                if (!string.IsNullOrEmpty(model.EmergencyFirstName) && !string.IsNullOrEmpty(model.EmergencyContactNumber))
                {
                    Console.WriteLine($"Step 7: Creating emergency contact...");
                    var emergencyContact = new Contact
                    {
                        FirstName = model.EmergencyFirstName,
                        LastName = model.EmergencyLastName,
                        MiddleName = model.EmergencyMiddleName,
                        Suffix = model.EmergencySuffix,
                        ContactNumber = model.EmergencyContactNumber,
                        Relationship = model.Relationship,  // Added - this field exists in the table
                        CreatedAt = DateTime.UtcNow
                    };

                    var insertedEmergencyContactResponse = await client.From<Contact>().Insert(emergencyContact);
                    var insertedEmergencyContact = insertedEmergencyContactResponse.Model;
                    Console.WriteLine($"? Step 7 Complete: Emergency contact created with ID: {insertedEmergencyContact.Id}");

                    // Update user record with emergency contact info (simplified - store as text)
                    var emergencyText = $"{model.EmergencyFirstName} {model.EmergencyLastName} - {model.EmergencyContactNumber}";
                    Console.WriteLine($"  Emergency contact stored: {emergencyText}");
                }

                // Step 8: Send password setup email
                Console.WriteLine($"Step 8: Sending password setup email...");
                Console.WriteLine($"  Email address: {model.Email}");
                try
                {
                    await _supabaseAuthService.SendPasswordSetupEmailAsync(model.Email);
                    Console.WriteLine($"? Step 8 Complete: Password setup email sent to {model.Email}");
                    Console.WriteLine($"  The teacher should receive an email with password setup instructions.");
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"? Step 8 FAILED: Unable to send password setup email");
                    Console.WriteLine($"  Error: {emailEx.Message}");
                    Console.WriteLine($"  Stack Trace: {emailEx.StackTrace}");
                    Console.WriteLine($"  ? WARNING: Teacher account was created but email was not sent!");
                    Console.WriteLine($"  ? Admin must manually resend the password setup email from the user management page.");
                    Console.WriteLine($"  ? Or use the 'Forgot Password' feature with email: {model.Email}");
                }

                Console.WriteLine($"\n??? TEACHER CREATION COMPLETE ???");
                Console.WriteLine($"  Teacher ID: {insertedTeacher.Id}");
                Console.WriteLine($"  Auth User ID: {supabaseUserId}");
                Console.WriteLine($"  Email: {model.Email}\n");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n??? TEACHER CREATION FAILED ???");
                Console.WriteLine($"  Error: {ex.Message}");
                Console.WriteLine($"  Stack Trace: {ex.StackTrace}\n");

                // Note: Auth user is already created at this point
                // Manual cleanup may be needed in Supabase Dashboard if teacher creation fails
                Console.WriteLine($"?? WARNING: Auth user may have been created. Manual cleanup may be required.");
                Console.WriteLine($"   Email: {model.Email}");

                throw new Exception($"Error creating teacher: {ex.Message}", ex);
            }
        }

        public async Task<SupabaseUserNew> GetTeacherByIdAsync(int teacherProfileId)
        {
            try
            {
                var client = await GetSupabaseClientAsync();

                // Get teacherProfile first
                var teacherProfile = await client.From<Teacher>()
                    .Where(x => x.Id == teacherProfileId)
                    .Single();

                // Then get the full user record
                var user = await client.From<SupabaseUserNew>()
                    .Where(x => x.UserTypeId == teacherProfile.TeacherId)
                    .Single();

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving teacher: {ex.Message}", ex);
            }
        }

        public async Task<SupabaseUserNew> GetTeacherByEmailAsync(string email)
        {
            try
            {
                var client = await GetSupabaseClientAsync();
                // Query users table directly by email
                var response = await client.From<SupabaseUserNew>()
                    .Where(x => x.Email == email)
                    .Single();

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving teacher by email: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateTeacherAsync(TeacherViewModel model)
        {
            try
            {
                var client = await GetSupabaseClientAsync();

                // Get the user record
                var existingUser = await GetTeacherByEmailAsync(model.Email);
                if (existingUser == null)
                {
                    return false;
                }

                // Update user table
                existingUser.FirstName = model.FirstName;
                existingUser.LastName = model.LastName;
                existingUser.MiddleName = model.MiddleName;
                existingUser.Suffix = model.Suffix;
                existingUser.ContactNumber = model.ContactNumber;

                await client.From<SupabaseUserNew>()
                    .Where(x => x.UserTypeId == existingUser.UserTypeId)
                    .Update(existingUser);

                // Update teacherProfile table
                var teacherProfile = await client.From<Teacher>()
                    .Where(x => x.TeacherId == existingUser.UserTypeId)
                    .Single();

                // Lookup department ID
                int? departmentId = null;
                try
                {
                    var deptQuery = await client.From<Department>()
                        .Where(x => x.DepartmentName == model.Department)
                        .Get();
                    departmentId = deptQuery?.Models?.FirstOrDefault()?.Id;
                }
                catch { }

                teacherProfile.DepartmentId = departmentId;  // Fixed: use int, not string

                await client.From<Teacher>()
                    .Where(x => x.TeacherId == existingUser.UserTypeId)
                    .Update(teacherProfile);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating teacher: {ex.Message}", ex);
            }
        }

    }
}
