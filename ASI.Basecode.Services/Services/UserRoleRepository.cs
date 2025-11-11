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
    /// Repository implementation for UserRole entity operations.
    /// Handles user-to-role assignments using Supabase queries.
    /// </summary>
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public UserRoleRepository(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public async Task<List<UserRole>> GetUserRolesByUserIdAsync(int userId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<UserRole>()
                    .Where(x => x.userId == userId)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching user roles for user {userId}: {ex.Message}");
                return new List<UserRole>();
            }
        }

        public async Task<List<UserRole>> GetUserRolesByRoleIdAsync(int roleId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<UserRole>()
                    .Where(x => x.roleId == roleId)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching user roles for role {roleId}: {ex.Message}");
                return new List<UserRole>();
            }
        }

        public async Task<UserRole> GetUserRoleByIdAsync(int userRoleId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<UserRole>()
                    .Where(x => x.id == userRoleId)
                    .Single();
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching user role {userRoleId}: {ex.Message}");
                return null;
            }
        }

        public async Task<UserRole> CreateUserRoleAsync(UserRole userRole)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<UserRole>()
                    .Insert(new List<UserRole> { userRole });
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating user role: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteUserRoleAsync(int userRoleId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                await client.From<UserRole>()
                    .Where(x => x.id == userRoleId)
                    .Delete();
                return true; // Successful if no exception
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting user role {userRoleId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UserHasRoleAsync(int userId, int roleId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<UserRole>()
                    .Where(x => x.userId == userId && x.roleId == roleId)
                    .Get();
                return response.Models.Any();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking if user {userId} has role {roleId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<UserRole>> GetAllUserRolesAsync()
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<UserRole>()
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching all user roles: {ex.Message}");
                return new List<UserRole>();
            }
        }
    }
}
