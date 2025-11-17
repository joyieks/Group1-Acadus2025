using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Service for audit logging functionality.
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly ISupabaseAuthService _supabaseAuthService;

        public AuditLogService(ISupabaseAuthService supabaseAuthService)
        {
            _supabaseAuthService = supabaseAuthService;
        }

        /// <summary>
        /// Logs an activity to the audit_logs table.
        /// </summary>
        public async Task LogActivityAsync(
            string userId,
            string userRole,
            string userName,
            string actionType,
            string actionDescription,
            long? courseId = null,
            string courseCode = null,
            string courseName = null,
            string studentId = null,
            string studentName = null,
            int? activityId = null,
            string activityTitle = null,
            string details = null,
            string metadata = null)
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                var auditLog = new AuditLogModel
                {
                    UserId = userId,
                    UserRole = userRole,
                    UserName = userName,
                    ActionType = actionType,
                    ActionDescription = actionDescription,
                    CourseId = courseId,
                    CourseCode = courseCode,
                    CourseName = courseName,
                    StudentId = studentId,
                    StudentName = studentName,
                    ActivityId = activityId,
                    ActivityTitle = activityTitle,
                    Details = details,
                    Metadata = metadata,
                    CreatedAt = DateTime.UtcNow
                };

                await client.From<AuditLogModel>().Insert(auditLog);
                Console.WriteLine($"Audit log created: {actionType} - {actionDescription}");
            }
            catch (Exception ex)
            {
                // Log error but don't throw - audit logging should not break the main flow
                Console.WriteLine($"Error logging audit activity: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Gets recent audit logs for a specific user.
        /// </summary>
        public async Task<List<AuditLogModel>> GetRecentLogsByUserAsync(string userId, int limit = 10)
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                var response = await client
                    .From<AuditLogModel>()
                    .Filter("userId", Supabase.Postgrest.Constants.Operator.Equals, userId)
                    .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Limit(limit)
                    .Get();

                return response?.Models?.ToList() ?? new List<AuditLogModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving audit logs for user: {ex.Message}");
                return new List<AuditLogModel>();
            }
        }

        /// <summary>
        /// Gets recent audit logs for a specific course.
        /// </summary>
        public async Task<List<AuditLogModel>> GetRecentLogsByCourseAsync(long courseId, int limit = 10)
        {
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                var response = await client
                    .From<AuditLogModel>()
                    .Filter("courseId", Supabase.Postgrest.Constants.Operator.Equals, courseId)
                    .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Limit(limit)
                    .Get();

                return response?.Models?.ToList() ?? new List<AuditLogModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving audit logs for course: {ex.Message}");
                return new List<AuditLogModel>();
            }
        }
    }
}

