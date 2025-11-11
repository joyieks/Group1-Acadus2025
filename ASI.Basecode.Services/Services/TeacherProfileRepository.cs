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
    /// Repository implementation for TeacherProfile entity operations.
    /// Handles teacher-specific profile data using Supabase queries.
    /// </summary>
    public class TeacherProfileRepository : ITeacherProfileRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public TeacherProfileRepository(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public async Task<List<TeacherProfile>> GetAllTeacherProfilesAsync()
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<TeacherProfile>().Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching all teacher profiles: {ex.Message}");
                return new List<TeacherProfile>();
            }
        }

        public async Task<TeacherProfile> GetTeacherProfileByIdAsync(int profileId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<TeacherProfile>()
                    .Where(x => x.id == profileId)
                    .Single();
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching teacher profile {profileId}: {ex.Message}");
                return null;
            }
        }

        public async Task<TeacherProfile> GetTeacherProfileByUserIdAsync(int userId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<TeacherProfile>()
                    .Where(x => x.userId == userId)
                    .Single();
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching teacher profile for user {userId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<TeacherProfile>> GetTeacherProfilesByDepartmentAsync(int departmentId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<TeacherProfile>()
                    .Where(x => x.departmentId == departmentId)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching teacher profiles for department {departmentId}: {ex.Message}");
                return new List<TeacherProfile>();
            }
        }

        public async Task<int> GetTeacherCountByDepartmentAsync(int departmentId)
        {
            try
            {
                var profiles = await GetTeacherProfilesByDepartmentAsync(departmentId);
                return profiles.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error counting teachers in department {departmentId}: {ex.Message}");
                return 0;
            }
        }

        public async Task<TeacherProfile> CreateTeacherProfileAsync(TeacherProfile teacherProfile)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<TeacherProfile>()
                    .Insert(new List<TeacherProfile> { teacherProfile });
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating teacher profile: {ex.Message}");
                return null;
            }
        }

        public async Task<TeacherProfile> UpdateTeacherProfileAsync(TeacherProfile teacherProfile)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<TeacherProfile>()
                    .Update(teacherProfile);
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating teacher profile: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UserHasTeacherProfileAsync(int userId)
        {
            try
            {
                var profile = await GetTeacherProfileByUserIdAsync(userId);
                return profile != null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking if user {userId} has teacher profile: {ex.Message}");
                return false;
            }
        }
    }
}
