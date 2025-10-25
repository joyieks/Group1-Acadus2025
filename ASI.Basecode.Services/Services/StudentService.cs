using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.Extensions.Configuration;
using Supabase;
using System;
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

                Console.WriteLine($"Step 2: Inserting student record into database...");
                var student = new Student
                {
                    SupabaseUserId = supabaseUserId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    MiddleName = model.MiddleName,
                    Suffix = model.Suffix,
                    Email = model.Email,
                    ContactNumber = model.ContactNumber,
                    YearLevel = model.YearLevel,
                    Program = model.Program,
                    Department = model.Department,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var insertedStudentResponse = await client.From<Student>().Insert(student);
                var insertedStudent = insertedStudentResponse.Model;

                Console.WriteLine($"✓ Step 2 Complete: Student record created with ID: {insertedStudent.Id}");

                Console.WriteLine($"Step 3: Creating address record...");
                var address = new Address
                {
                    HouseNumber = model.HouseNumber,
                    StreetName = model.StreetName,
                    Subdivision = model.Subdivision,
                    Barangay = model.Barangay,
                    City = model.City,
                    Province = model.Province,
                    ZipCode = model.ZipCode,
                    CreatedAt = DateTime.UtcNow
                };

                var insertedAddressResponse = await client.From<Address>().Insert(address);
                var insertedAddress = insertedAddressResponse.Model;

                Console.WriteLine($"✓ Step 3 Complete: Address created with ID: {insertedAddress.Id}");

                Console.WriteLine($"Step 4: Linking student to address...");
                var studentAddress = new StudentAddress
                {
                    StudentId = insertedStudent.Id,
                    AddressId = insertedAddress.Id,
                    AddressType = "current",
                    IsPrimary = true,
                    CreatedAt = DateTime.UtcNow
                };

                await client.From<StudentAddress>().Insert(studentAddress);
                Console.WriteLine($"✓ Step 4 Complete: Student-Address link created");

                Console.WriteLine($"Step 5: Creating emergency contact...");
                var emergencyContact = new Contact
                {
                    FirstName = model.EmergencyFirstName,
                    LastName = model.EmergencyLastName,
                    MiddleName = model.EmergencyMiddleName,
                    Suffix = model.EmergencySuffix,
                    ContactNumber = model.EmergencyContactNumber,
                    CreatedAt = DateTime.UtcNow
                };

                var insertedEmergencyContactResponse = await client.From<Contact>().Insert(emergencyContact);
                var insertedEmergencyContact = insertedEmergencyContactResponse.Model;

                Console.WriteLine($"✓ Step 5 Complete: Emergency contact created with ID: {insertedEmergencyContact.Id}");

                Console.WriteLine($"Step 6: Linking student to emergency contact...");
                var studentEmergencyContact = new StudentEmergencyContact
                {
                    StudentId = insertedStudent.Id,
                    ContactId = insertedEmergencyContact.Id,
                    Relationship = model.Relationship,
                    IsPrimary = true,
                    CreatedAt = DateTime.UtcNow
                };

                await client.From<StudentEmergencyContact>().Insert(studentEmergencyContact);
                Console.WriteLine($"✓ Step 6 Complete: Student-Emergency Contact link created");

                Console.WriteLine($"Step 7: Sending password setup email...");
                try
                {
                    await _supabaseAuthService.SendPasswordSetupEmailAsync(model.Email);
                    Console.WriteLine($"✓ Step 7 Complete: Password setup email sent to {model.Email}");
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"⚠ Step 7 Warning: Failed to send password setup email: {emailEx.Message}");
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

        public async Task<Student> GetStudentByIdAsync(int id)
        {
            try
            {
                var client = await GetSupabaseClientAsync();
                var response = await client.From<Student>()
                    .Where(x => x.Id == id)
                    .Single();

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving student: {ex.Message}", ex);
            }
        }

        public async Task<Student> GetStudentByEmailAsync(string email)
        {
            try
            {
                var client = await GetSupabaseClientAsync();
                var response = await client.From<Student>()
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

                var existingStudent = await GetStudentByEmailAsync(model.Email);
                if (existingStudent == null)
                {
                    return false;
                }

                existingStudent.FirstName = model.FirstName;
                existingStudent.LastName = model.LastName;
                existingStudent.MiddleName = model.MiddleName;
                existingStudent.Suffix = model.Suffix;
                existingStudent.ContactNumber = model.ContactNumber;
                existingStudent.YearLevel = model.YearLevel;
                existingStudent.Program = model.Program;
                existingStudent.Department = model.Department;
                existingStudent.UpdatedAt = DateTime.UtcNow;

                await client.From<Student>().Update(existingStudent);

                var studentAddress = await client.From<StudentAddress>()
                    .Where(x => x.StudentId == existingStudent.Id && x.IsPrimary == true)
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

                var studentEmergencyContact = await client.From<StudentEmergencyContact>()
                    .Where(x => x.StudentId == existingStudent.Id && x.IsPrimary == true)
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

                var student = await GetStudentByIdAsync(id);
                if (student == null)
                {
                    return false;
                }

                await _supabaseAuthService.DeleteUserAsync(student.SupabaseUserId);
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