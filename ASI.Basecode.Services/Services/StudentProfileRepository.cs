using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data;
using ASI.Basecode.Services.Interfaces;

namespace ASI.Basecode.Services
{
    /// <summary>
    /// Repository implementation for StudentProfile entity operations.
    /// Handles student-specific profile data using Supabase queries.
    /// </summary>
    public class StudentProfileRepository : IStudentProfileRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public StudentProfileRepository(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public async Task<List<StudentProfile>> GetAllStudentProfilesAsync()
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<StudentProfile>().Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching all student profiles: {ex.Message}");
                return new List<StudentProfile>();
            }
        }

        public async Task<StudentProfile> GetStudentProfileByIdAsync(int profileId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<StudentProfile>()
                    .Where(x => x.id == profileId)
                    .Single();
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching student profile {profileId}: {ex.Message}");
                return null;
            }
        }

        public async Task<StudentProfile> GetStudentProfileByUserIdAsync(int userId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<StudentProfile>()
                    .Where(x => x.userId == userId)
                    .Single();
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching student profile for user {userId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<StudentProfile>> GetStudentProfilesByProgramAsync(int programId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<StudentProfile>()
                    .Where(x => x.programId == programId)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching student profiles for program {programId}: {ex.Message}");
                return new List<StudentProfile>();
            }
        }

        public async Task<List<StudentProfile>> GetStudentProfilesByDepartmentAsync(int departmentId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<StudentProfile>()
                    .Where(x => x.departmentId == departmentId)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching student profiles for department {departmentId}: {ex.Message}");
                return new List<StudentProfile>();
            }
        }

        public async Task<List<StudentProfile>> GetStudentProfilesByYearLevelAsync(int yearLevel)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<StudentProfile>()
                    .Where(x => x.yearLevel == yearLevel)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching student profiles for year level {yearLevel}: {ex.Message}");
                return new List<StudentProfile>();
            }
        }

        public async Task<List<StudentProfile>> GetStudentProfilesByProgramAndYearAsync(int programId, int yearLevel)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<StudentProfile>()
                    .Where(x => x.programId == programId && x.yearLevel == yearLevel)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching student profiles for program {programId} and year {yearLevel}: {ex.Message}");
                return new List<StudentProfile>();
            }
        }

        public async Task<StudentProfile> CreateStudentProfileAsync(StudentProfile studentProfile)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<StudentProfile>()
                    .Insert(new List<StudentProfile> { studentProfile });
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating student profile: {ex.Message}");
                return null;
            }
        }

        public async Task<StudentProfile> UpdateStudentProfileAsync(StudentProfile studentProfile)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<StudentProfile>()
                    .Update(studentProfile);
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating student profile: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetStudentCountByProgramAsync(int programId)
        {
            try
            {
                var profiles = await GetStudentProfilesByProgramAsync(programId);
                return profiles.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error counting students in program {programId}: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> UserHasStudentProfileAsync(int userId)
        {
            try
            {
                var profile = await GetStudentProfileByUserIdAsync(userId);
                return profile != null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking if user {userId} has student profile: {ex.Message}");
                return false;
            }
        }
    }
}
