using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.Extensions.Configuration;
using Supabase;
using System;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IConfiguration _configuration;
        private Supabase.Client _supabaseClient;

        public TeacherService(ISupabaseAuthService supabaseAuthService, IConfiguration configuration)
        {
            _supabaseAuthService = supabaseAuthService;
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

        public async Task<bool> CreateTeacherAsync(TeacherViewModel model)
        {
            try
            {
                // Generate a secure random password that will be immediately reset
                var secureRandomPassword = Guid.NewGuid().ToString() + "Aa1!";
                
                // Create user in Supabase Auth
                var supabaseUserId = await _supabaseAuthService.CreateUserAsync(
                    model.Email, 
                    secureRandomPassword, 
                    model.FirstName, 
                    model.LastName
                );

                Console.WriteLine($"Created user account for {model.Email}. Password reset email will be sent.");

                var client = await GetSupabaseClientAsync();

                // Create teacher record in Supabase database
                var teacher = new Teacher
                {
                    SupabaseUserId = supabaseUserId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    MiddleName = model.MiddleName,
                    Suffix = model.Suffix,
                    Email = model.Email,
                    ContactNumber = model.ContactNumber,
                    IdNumber = model.IdNumber,
                    Department = model.Department,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var insertedTeacherResponse = await client.From<Teacher>().Insert(teacher);
                var insertedTeacher = insertedTeacherResponse.Model;

                // Create address record
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

                // Link teacher to address
                var teacherAddress = new TeacherAddress
                {
                    TeacherId = insertedTeacher.Id,
                    AddressId = insertedAddress.Id,
                    AddressType = "current",
                    IsPrimary = true,
                    CreatedAt = DateTime.UtcNow
                };

                await client.From<TeacherAddress>().Insert(teacherAddress);

                // Create emergency contact record
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

                // Link teacher to emergency contact
                var teacherEmergencyContact = new TeacherEmergencyContact
                {
                    TeacherId = insertedTeacher.Id,
                    ContactId = insertedEmergencyContact.Id,
                    Relationship = model.Relationship,
                    IsPrimary = true,
                    CreatedAt = DateTime.UtcNow
                };

                await client.From<TeacherEmergencyContact>().Insert(teacherEmergencyContact);

                // Send secure password setup email
                try
                {
                    await _supabaseAuthService.SendPasswordSetupEmailAsync(model.Email);
                    Console.WriteLine($"Password setup email sent to {model.Email}");
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"Warning: Failed to send password setup email for {model.Email}: {emailEx.Message}");
                }

                return true;
            }
            catch (Exception ex)
            {
                // Clean up auth user if teacher creation fails
                try
                {
                    await _supabaseAuthService.DeleteUserAsync(model.Email);
                }
                catch
                {
                    // Log cleanup failure but don't throw
                }
                
                throw new Exception($"Error creating teacher: {ex.Message}", ex);
            }
        }

        public async Task<Teacher> GetTeacherByIdAsync(int id)
        {
            try
            {
                var client = await GetSupabaseClientAsync();
                var response = await client.From<Teacher>()
                    .Where(x => x.Id == id)
                    .Single();
                
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving teacher: {ex.Message}", ex);
            }
        }

        public async Task<Teacher> GetTeacherByEmailAsync(string email)
        {
            try
            {
                var client = await GetSupabaseClientAsync();
                var response = await client.From<Teacher>()
                    .Where(x => x.Email == email)
                    .Single();
                
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving teacher by email: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateTeacherAsync(Teacher teacher)
        {
            try
            {
                var client = await GetSupabaseClientAsync();
                teacher.UpdatedAt = DateTime.UtcNow;
                
                await client.From<Teacher>()
                    .Where(x => x.Id == teacher.Id)
                    .Update(teacher);
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating teacher: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteTeacherAsync(int id)
        {
            try
            {
                var client = await GetSupabaseClientAsync();
                
                // Get teacher to get supabase_user_id for auth cleanup
                var teacher = await GetTeacherByIdAsync(id);
                
                // Delete teacher (cascade will handle related records)
                await client.From<Teacher>()
                    .Where(x => x.Id == id)
                    .Delete();
                
                // Clean up auth user
                try
                {
                    await _supabaseAuthService.DeleteUserAsync(teacher.SupabaseUserId);
                }
                catch
                {
                    // Log cleanup failure but don't throw
                }
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting teacher: {ex.Message}", ex);
            }
        }
    }
}
