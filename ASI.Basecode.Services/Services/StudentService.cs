using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.Extensions.Configuration;
using Supabase;
using System;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class StudentService : IStudentService
    {
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IConfiguration _configuration;
        private Supabase.Client _supabaseClient;

        public StudentService(ISupabaseAuthService supabaseAuthService, IConfiguration configuration)
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

        public async Task<bool> CreateStudentAsync(StudentViewModel model)
        {
            try
            {
                // Generate a secure random password that will be immediately reset
                // This password is never sent to the user - they will use the password reset link
                var secureRandomPassword = Guid.NewGuid().ToString() + "Aa1!"; // Meets complexity requirements
                
                // Create user in Supabase Auth
                var supabaseUserId = await _supabaseAuthService.CreateUserAsync(
                    model.Email, 
                    secureRandomPassword, 
                    model.FirstName, 
                    model.LastName
                );

                Console.WriteLine($"Created user account for {model.Email}. Password reset email will be sent.");

                var client = await GetSupabaseClientAsync();

                // Create student record in Supabase database
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

                // Link student to address
                var studentAddress = new StudentAddress
                {
                    StudentId = insertedStudent.Id,
                    AddressId = insertedAddress.Id,
                    AddressType = "current",
                    IsPrimary = true,
                    CreatedAt = DateTime.UtcNow
                };

                await client.From<StudentAddress>().Insert(studentAddress);

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

                // Link student to emergency contact
                var studentEmergencyContact = new StudentEmergencyContact
                {
                    StudentId = insertedStudent.Id,
                    ContactId = insertedEmergencyContact.Id,
                    Relationship = model.Relationship,
                    IsPrimary = true,
                    CreatedAt = DateTime.UtcNow
                };

                await client.From<StudentEmergencyContact>().Insert(studentEmergencyContact);

                // Send secure password setup email (no password included)
                // This sends a one-time link that allows the student to set their own password
                try
                {
                    // Send password reset email through Supabase Auth
                    await _supabaseAuthService.SendPasswordSetupEmailAsync(model.Email);
                    
                    Console.WriteLine($"Password setup email sent to {model.Email}");
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail the student creation
                    Console.WriteLine($"Warning: Failed to send password setup email for {model.Email}: {emailEx.Message}");
                    
                    // Note: Student account is still created, admin can resend setup email
                }

                return true;
            }
            catch (Exception ex)
            {
                // If student creation fails, we should clean up the auth user
                try
                {
                    // This is a simplified cleanup - in production you'd want more robust error handling
                    await _supabaseAuthService.DeleteUserAsync(model.Email);
                }
                catch
                {
                    // Log the cleanup failure but don't throw
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
                
                // First, get the existing student
                var existingStudent = await GetStudentByEmailAsync(model.Email);
                if (existingStudent == null)
                {
                    return false;
                }

                // Update the student record
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

                // Update address (get existing address for this student)
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

                // Update emergency contact (get existing emergency contact for this student)
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

                // Update relationship
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
                
                // Get the student first to get the Supabase user ID
                var student = await GetStudentByIdAsync(id);
                if (student == null)
                {
                    return false;
                }

                // Delete from Supabase Auth
                await _supabaseAuthService.DeleteUserAsync(student.SupabaseUserId);

                // Delete from database
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
