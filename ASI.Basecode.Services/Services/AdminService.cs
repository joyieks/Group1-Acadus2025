using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class AdminService : IAdminService
    {
        private readonly ISupabaseAuthService _supabaseAuthService;

        public AdminService(ISupabaseAuthService supabaseAuthService)
        {
            _supabaseAuthService = supabaseAuthService;
        }

        public async Task<(int TotalStudents, int TotalInstructors, int TotalCourses)> GetDashboardStatisticsAsync()
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                // Get all user roles
                var allUserRoles = await client
                    .From<UserRole>()
                    .Get();

                // Get all users
                var allUsers = await client
                    .From<SupabaseUserNew>()
                    .Get();

                var userRolesList = allUserRoles?.Models ?? new List<UserRole>();
                var usersList = allUsers?.Models ?? new List<SupabaseUserNew>();

                // Count students: user_roles with role_id = 1 AND corresponding user has isActive = true
                var studentCount = userRolesList
                    .Where(ur => ur.RoleId == 1) // Student role
                    .Count(ur => usersList.Any(u => u.UserTypeId == ur.UserId && u.IsActive == true));

                // Count instructors: user_roles with role_id = 2 AND corresponding user has isActive = true
                var instructorCount = userRolesList
                    .Where(ur => ur.RoleId == 2) // Teacher/Instructor role
                    .Count(ur => usersList.Any(u => u.UserTypeId == ur.UserId && u.IsActive == true));

                // Get total courses
                var coursesQuery = await client
                    .From<CourseModel>()
                    .Get();

                var totalCourses = coursesQuery?.Models?.Count() ?? 0;

                return (studentCount, instructorCount, totalCourses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting dashboard statistics: {ex.Message}");
                return (0, 0, 0);
            }
        }
    }
}