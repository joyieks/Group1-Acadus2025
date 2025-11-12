using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.Extensions.Configuration;
using Supabase;
using System;
using System.Linq;  // Added for FirstOrDefault
using System.Net.Http;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class StudentService : IStudentService
    {
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IConfiguration _configuration;
        private Supabase.Client _supabaseClient;
        private static HttpClient _httpClient;

        public StudentService(ISupabaseAuthService supabaseAuthService, IConfiguration configuration)
        {
            _supabaseAuthService = supabaseAuthService;
            _configuration = configuration;
        }

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
                    Console.WriteLine("[StudentService] ✓ Custom HttpClient created with SSL validation bypassed");
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

                Console.WriteLine($"[StudentService] Initializing Supabase Client");
                Console.WriteLine($"  URL: {url}");
                Console.WriteLine($"  Development Mode: {isDevelopment}");

                var options = new SupabaseOptions
                {
                    AutoConnectRealtime = false,
                    AutoRefreshToken = true
                };

                _supabaseClient = new Supabase.Client(url, serviceRoleKey, options);

                // Inject custom HttpClient
                var httpClientProperty = _supabaseClient.GetType().GetProperty("HttpClient");
                if (httpClientProperty != null && isDevelopment)
                {
                    httpClientProperty.SetValue(_supabaseClient, GetHttpClient());
                    Console.WriteLine("  ✓ Custom HttpClient injected with SSL validation bypassed");
                }

                await _supabaseClient.InitializeAsync();
                Console.WriteLine("  ✓ Supabase Client initialized successfully");
            }
            return _supabaseClient;
        }

        public async Task<bool> CreateStudentAsync(StudentViewModel model)
        {
            try
            {
                Console.WriteLine($"\n=== CREATING STUDENT: {model.FirstName} {model.LastName} ===");

                var secureRandomPassword = Guid.NewGuid().ToString() + "Aa1!";

                Console.WriteLine($"Step 1: Creating Supabase Auth user...");
                var supabaseUserId = await _supabaseAuthService.CreateUserAsync(
                    model.Email,
                    secureRandomPassword,
                    model.FirstName,
                    model.LastName
                );

                Console.WriteLine($"✓ Step 1 Complete: Auth user created with ID: {supabaseUserId}");

                var client = await GetSupabaseClientAsync();

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
                    IsActive = true,
                    ProfilePictureUrl = null,
                    Address = null,
                    EmergencyContact = null
                };

                var insertedUserResponse = await client.From<SupabaseUserNew>().Insert(userRecord);
                var insertedUser = insertedUserResponse.Model;
                Console.WriteLine($"✓ Step 2 Complete: User record created with ID: {insertedUser.Id}");

                // Step 3: Lookup program and department IDs
                Console.WriteLine($"Step 3: Looking up program and department IDs...");
                int? programId = null;
                int? departmentId = null;

                try
                {
                    // Try to find program by name
                    var programQuery = await client.From<ASI.Basecode.Data.Models.Program>()
                        .Where(x => x.ProgramName == model.Program)
                        .Get();
                    var programRecord = programQuery?.Models?.FirstOrDefault();
                    programId = programRecord?.Id;
                    Console.WriteLine($"  Program lookup: {(programId.HasValue ? $"Found ID {programId}" : "Not found, will use null")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Warning: Program lookup failed: {ex.Message}");
                }

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
                Console.WriteLine($"✓ Step 3 Complete: Program ID = {programId}, Department ID = {departmentId}");

                // Step 4: Create studentProfile record (only stores studentId, yearLevel, programId, departmentId)
                Console.WriteLine($"Step 4: Creating studentProfile record...");
                var student = new Student
                {
                    StudentId = supabaseUserId,   // References users.userTypeId
                    YearLevel = model.YearLevel,
                    ProgramId = programId,  // FK to programs table
                    DepartmentId = departmentId,  // FK to departments table
                    CreatedAt = DateTime.UtcNow
                };

                var insertedStudentResponse = await client.From<Student>().Insert(student);
                var insertedStudent = insertedStudentResponse.Model;
                Console.WriteLine($"✓ Step 4 Complete: StudentProfile created with ID: {insertedStudent.Id}");

                // Step 5: Lookup Student role and assign in user_roles table
                Console.WriteLine($"Step 5: Looking up Student role and assigning in user_roles table...");
                
                // Lookup Student role by name to get ID
                int studentRoleId = 1; // Default to 1 if lookup fails
                try
                {
                    var roleQuery = await client.From<Role>()
                     .Where(x => x.RoleName == "Student")
    .Get();
        var roleRecord = roleQuery?.Models?.FirstOrDefault();
              if (roleRecord != null)
   {
            studentRoleId = roleRecord.Id;
             Console.WriteLine($"  Role lookup: Found 'Student' role with ID {studentRoleId}");
                }
        else
          {
         Console.WriteLine($"  Warning: Student role not found, using default ID 1");
          }
 }
        catch (Exception ex)
       {
            Console.WriteLine($"  Warning: Role lookup failed: {ex.Message}, using default ID 1");
    }

              var userRole = new UserRole
     {
      UserId = supabaseUserId, // Supabase Auth UUID
         RoleId = studentRoleId, // Now an int referencing roles.id
               CreatedAt = DateTime.UtcNow
           };

             await client.From<UserRole>().Insert(userRole);
     Console.WriteLine($"✓ Step 5 Complete: Student role (ID {studentRoleId}) assigned");

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
                    Console.WriteLine($"✓ Step 6 Complete: Address created with ID: {insertedAddress.Id}");

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
                    Console.WriteLine($"✓ Step 7 Complete: Emergency contact created with ID: {insertedEmergencyContact.Id}");

                    // Update user record with emergency contact info (simplified - store as text)
                    var emergencyText = $"{model.EmergencyFirstName} {model.EmergencyLastName} - {model.EmergencyContactNumber}";
                    Console.WriteLine($"  Emergency contact stored: {emergencyText}");
                }

                // Step 8: Send password setup email
                Console.WriteLine($"Step 8: Sending password setup email...");
                try
                {
                    await _supabaseAuthService.SendPasswordSetupEmailAsync(model.Email);
                    Console.WriteLine($"✓ Step 8 Complete: Password setup email sent to {model.Email}");
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"⚠ Step 8 Warning: Failed to send password setup email: {emailEx.Message}");
                    Console.WriteLine($"  Note: Student account is still created. Admin can resend email manually.");
                }

                Console.WriteLine($"\n✓✓✓ STUDENT CREATION COMPLETE ✓✓✓");
                Console.WriteLine($"  Student ID: {insertedStudent.Id}");
                Console.WriteLine($"  Auth User ID: {supabaseUserId}");
                Console.WriteLine($"  Email: {model.Email}\n");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗✗✗ STUDENT CREATION FAILED ✗✗✗");
                Console.WriteLine($"  Error: {ex.Message}");
                Console.WriteLine($"  Stack Trace: {ex.StackTrace}\n");

                try
                {
                    Console.WriteLine($"Attempting to clean up auth user...");
                    await _supabaseAuthService.DeleteUserAsync(model.Email);
                    Console.WriteLine($"✓ Auth user cleanup successful");
                }
                catch (Exception cleanupEx)
                {
                    Console.WriteLine($"⚠ Auth user cleanup failed: {cleanupEx.Message}");
                }

                throw new Exception($"Error creating student: {ex.Message}", ex);
            }
        }

        public async Task<SupabaseUserNew> GetStudentByIdAsync(int studentProfileId)
        {
            try
            {
                var client = await GetSupabaseClientAsync();
       
                // Get studentProfile first
                var studentProfile = await client.From<Student>()
 .Where(x => x.Id == studentProfileId)
      .Single();

                // Then get the full user record
var user = await client.From<SupabaseUserNew>()
        .Where(x => x.UserTypeId == studentProfile.StudentId)
          .Single();

 return user;
          }
       catch (Exception ex)
            {
             throw new Exception($"Error retrieving student: {ex.Message}", ex);
          }
    }

        public async Task<SupabaseUserNew> GetStudentByEmailAsync(string email)
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
    throw new Exception($"Error retrieving student by email: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateStudentAsync(StudentViewModel model)
        {
     try
      {
         var client = await GetSupabaseClientAsync();

   // Get the user record
  var existingUser = await GetStudentByEmailAsync(model.Email);
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

      // Update studentProfile table
     var studentProfile = await client.From<Student>()
  .Where(x => x.StudentId == existingUser.UserTypeId)
          .Single();

   // Lookup program and department IDs
       int? programId = null;
    int? departmentId = null;

 try
   {
         var programQuery = await client.From<ASI.Basecode.Data.Models.Program>()
       .Where(x => x.ProgramName == model.Program)
     .Get();
  programId = programQuery?.Models?.FirstOrDefault()?.Id;
}
   catch { }

   try
   {
var deptQuery = await client.From<Department>()
.Where(x => x.DepartmentName == model.Department)
   .Get();
  departmentId = deptQuery?.Models?.FirstOrDefault()?.Id;
}
   catch { }

studentProfile.YearLevel = model.YearLevel;
          studentProfile.ProgramId = programId;  // Fixed: use ProgramId
    studentProfile.DepartmentId = departmentId;  // Fixed: use int, not string

         await client.From<Student>()
           .Where(x => x.StudentId == existingUser.UserTypeId)
      .Update(studentProfile);

       // Update address if exists
   try
             {
       var studentAddress = await client.From<StudentAddress>()
     .Where(x => x.StudentId == studentProfile.Id && x.IsPrimary == true)
   .Single();

      var address = await client.From<Address>()
      .Where(x => x.Id == studentAddress.AddressId)
 .Single();

            address.HouseNumber = model.HouseNumber;
         address.StreetName = model.StreetName;
    address.Subdivision = model.Subdivision;
         address.Barangay = model.Barangay;
                 address.City = model.City;
   address.Province = model.Province;
              address.ZipCode = model.ZipCode;

            await client.From<Address>().Update(address);
      }
                catch
       {
       // Address doesn't exist or error occurred, skip
       }

  // Update emergency contact if exists
    try
       {
        var studentEmergencyContact = await client.From<StudentEmergencyContact>()
   .Where(x => x.StudentId == studentProfile.Id && x.IsPrimary == true)
    .Single();

       var emergencyContact = await client.From<Contact>()
            .Where(x => x.Id == studentEmergencyContact.ContactId)
              .Single();

      emergencyContact.FirstName = model.EmergencyFirstName;
      emergencyContact.LastName = model.EmergencyLastName;
          emergencyContact.MiddleName = model.EmergencyMiddleName;
      emergencyContact.Suffix = model.EmergencySuffix;
        emergencyContact.ContactNumber = model.EmergencyContactNumber;

     await client.From<Contact>().Update(emergencyContact);

          studentEmergencyContact.Relationship = model.Relationship;
            await client.From<StudentEmergencyContact>().Update(studentEmergencyContact);
      }
      catch
        {
               // Emergency contact doesn't exist or error occurred, skip
     }

        return true;
 }
       catch (Exception ex)
   {
     throw new Exception($"Error updating student: {ex.Message}", ex);
      }
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
       try
            {
     var client = await GetSupabaseClientAsync();

      // Get studentProfile first
    var studentProfile = await client.From<Student>()
  .Where(x => x.Id == id)
 .Single();

      if (studentProfile == null)
        {
      return false;
        }

   // Delete from Supabase Auth
           await _supabaseAuthService.DeleteUserAsync(studentProfile.StudentId);
    
        // Delete studentProfile (cascade should handle related records)
                await client.From<Student>().Where(x => x.Id == id).Delete();

        return true;
   }
       catch (Exception ex)
            {
                throw new Exception($"Error deleting student: {ex.Message}", ex);
  }
        }
    }
}