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

                Console.WriteLine($"=== AdminService Debug ===");
                Console.WriteLine($"Total user_roles records: {userRolesList.Count}");
                Console.WriteLine($"Total users records: {usersList.Count}");

                // Count ACTIVE students: 
                // - user_roles.roleId == 1 (student role)
                // - AND corresponding user (matching userTypeId) has isActive == true
                var studentCount = userRolesList
                    .Where(ur => ur.RoleId == 1) // Student role
                    .Count(ur => usersList.Any(u => u.UserTypeId == ur.UserId && u.IsActive == true));

                Console.WriteLine($"Students (roleId=1, isActive=true) count: {studentCount}");
                Console.WriteLine($"  Details: {userRolesList.Where(ur => ur.RoleId == 1).Count()} total with roleId=1");

                // Count ACTIVE instructors:
                // - user_roles.roleId == 2 (instructor role)
                // - AND corresponding user (matching userTypeId) has isActive == true
                var instructorCount = userRolesList
                    .Where(ur => ur.RoleId == 2) // Teacher/Instructor role
                    .Count(ur => usersList.Any(u => u.UserTypeId == ur.UserId && u.IsActive == true));

                Console.WriteLine($"Instructors (roleId=2, isActive=true) count: {instructorCount}");
                Console.WriteLine($"  Details: {userRolesList.Where(ur => ur.RoleId == 2).Count()} total with roleId=2");

                // Get courses with Active status
                var coursesQuery = await client
                    .From<CourseModel>()
                    .Get();

                var coursesList = coursesQuery?.Models ?? new List<CourseModel>();
                
                // Filter courses where Status == "Active"
                var totalCourses = coursesList
                    .Where(c => c.Status == "Active")
                    .Count();

                Console.WriteLine($"Total active courses: {totalCourses}");
                Console.WriteLine($"  Details: {coursesList.Count} total courses in database");
                Console.WriteLine($"=== End Debug ===");

                return (studentCount, instructorCount, totalCourses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting dashboard statistics: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return (0, 0, 0);
            }
        }
    }
}