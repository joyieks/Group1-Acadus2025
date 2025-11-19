using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace ASI.Basecode.Data.Repositories
{
    public class TeacherCourseActivityRepository : ITeacherCourseActivityRepository
    {
        private readonly Client _client;

        public TeacherCourseActivityRepository(Client supabaseClient)
        {
            _client = supabaseClient;
        }

        public async Task<List<ActivityModel>> GetActivitiesByCourseAsync(int courseId)
        {
            // Get all activities for the course (teachers see all activities)
            var response = await _client
                .From<ActivityModel>()
                .Filter("courseId", Operator.Equals, (long)courseId)
                .Get();

            return response.Models?.OrderBy(a => a.DueDate).ToList() ?? new List<ActivityModel>();
        }

        public async Task<ActivityModel> GetActivityByIdAsync(int activityId)
        {
            var response = await _client
                .From<ActivityModel>()
                .Filter("id", Operator.Equals, activityId)
                .Get();

            return response.Models?.FirstOrDefault();
        }

        public async Task CreateActivityAsync(ActivityModel activity)
        {
            await _client.From<ActivityModel>().Insert(activity);
        }

        public async Task UpdateActivityAsync(ActivityModel activity)
        {
            await activity.Update<ActivityModel>();
        }

        public async Task DeleteActivityAsync(int activityId)
        {
            var activity = await GetActivityByIdAsync(activityId);
            if (activity != null)
            {
                await activity.Delete<ActivityModel>();
            }
        }

        public async Task<List<SupabaseUserNew>> GetStudentsByCourseIdAsync(int courseId)
        {
            try
            {
                Console.WriteLine($"=== GetStudentsByCourseIdAsync START ===");
                Console.WriteLine($"CourseId: {courseId}");

                // Get ALL enrollments for this course (don't filter by status in query)
                var enrollmentsRes = await _client
                    .From<EnrollmentModel>()
                    .Filter("course_id", Operator.Equals, (long)courseId)
                    .Get();

                var allEnrollments = enrollmentsRes.Models ?? new List<EnrollmentModel>();
                Console.WriteLine($"Total enrollments found: {allEnrollments.Count}");

                // Log all status values to see what's in the database
                var statusValues = allEnrollments
                    .Where(e => !string.IsNullOrEmpty(e.Status))
                    .Select(e => e.Status)
                    .Distinct()
                    .ToList();
                Console.WriteLine($"Unique status values: {string.Join(", ", statusValues)}");

                // Filter for active enrollments - match the same logic as StudentTableViewComponent
                // Only check Status field (case-insensitive), don't check DroppedAt to match student list behavior
                var activeEnrollments = allEnrollments
                    .Where(e => !string.IsNullOrEmpty(e.Status) && 
                               (e.Status == "Active" || e.Status.Equals("active", StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                Console.WriteLine($"Active enrollments after filtering: {activeEnrollments.Count}");

                var studentIds = activeEnrollments
                    .Select(e => e.StudentId)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .ToList();

                Console.WriteLine($"Unique student IDs: {studentIds.Count}");
                if (studentIds.Any())
                {
                    Console.WriteLine($"Sample student IDs: {string.Join(", ", studentIds.Take(5))}");
                }

                if (!studentIds.Any())
                {
                    Console.WriteLine("No student IDs found, returning empty list");
                    return new List<SupabaseUserNew>();
                }

                // Get users who are enrolled - use the same approach as StudentTableViewComponent
                // First, get all students (users with student role) - same as GetStudentsAsync()
                var userRolesQuery = await _client
                    .From<UserRoleModel>()
                    .Get();
                
                var allUserRoles = userRolesQuery?.Models ?? new List<UserRoleModel>();
                var studentUserTypeIds = allUserRoles
                    .Where(ur => ur.RoleId == "1") // Students have roleId = "1" (same as GetStudentsAsync)
                    .Select(ur => ur.UserId)
                    .Distinct()
                    .ToList();
                
                Console.WriteLine($"Found {studentUserTypeIds.Count} users with Student role");

                // Get all users
                var allUsersRes = await _client
                    .From<SupabaseUserNew>()
                    .Get();

                var allUsers = allUsersRes.Models ?? new List<SupabaseUserNew>();
                Console.WriteLine($"Total users in database: {allUsers.Count}");

                // Filter to only students (users with student role) - same as GetStudentsAsync()
                var allStudents = allUsers
                    .Where(u => !string.IsNullOrEmpty(u.UserTypeId) && 
                               studentUserTypeIds.Contains(u.UserTypeId) && 
                               (u.IsActive == null || u.IsActive == true))
                    .ToList();
                
                Console.WriteLine($"Filtered to {allStudents.Count} active students");

                // Match enrollments to students (same logic as StudentTableViewComponent)
                var users = allStudents
                    .Where(s => studentIds.Contains(s.UserTypeId))
                    .ToList();

                Console.WriteLine($"Found {users.Count} enrolled students matching {studentIds.Count} enrollment student IDs");

                Console.WriteLine($"=== GetStudentsByCourseIdAsync END: Returning {users.Count} students ===");
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetStudentsByCourseIdAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<SupabaseUserNew>();
            }
        }

        public async Task<List<ActivitySubmissionModel>> GetSubmissionsByCourseAsync(int courseId)
        {
            // Get all activities for this course
            var activities = await GetActivitiesByCourseAsync(courseId);
            var activityIds = activities.Select(a => a.Id).ToList();

            if (!activityIds.Any())
                return new List<ActivitySubmissionModel>();

            // Get submissions for these activities
            var response = await _client
                .From<ActivitySubmissionModel>()
                .Filter("activityId", Operator.In, activityIds.Cast<object>().ToList())
                .Get();

            var submissions = response.Models?.ToList() ?? new List<ActivitySubmissionModel>();
            
            // Debug: Log submission content retrieval
            Console.WriteLine($"=== GetSubmissionsByCourseAsync ===");
            Console.WriteLine($"Found {submissions.Count} submissions");
            foreach (var sub in submissions.Take(5))
            {
                Console.WriteLine($"  Submission ID {sub.Id}: ActivityId={sub.ActivityId}, StudentId={sub.StudentId}");
                Console.WriteLine($"    SubmissionContent: {(string.IsNullOrEmpty(sub.SubmissionContent) ? "NULL/EMPTY" : $"Length={sub.SubmissionContent.Length}, Preview={sub.SubmissionContent.Substring(0, Math.Min(50, sub.SubmissionContent.Length))}...")}");
            }
            
            return submissions;
        }

        public async Task<ActivitySubmissionModel> GetSubmissionAsync(int activityId, string studentId)
        {
            var response = await _client
                .From<ActivitySubmissionModel>()
                .Filter("activityId", Operator.Equals, activityId)
                .Get();

            var submission = response.Models?
                .FirstOrDefault(s => s.StudentId == studentId);
            
            // Debug: Log retrieved submission
            if (submission != null)
            {
                Console.WriteLine($"=== GetSubmissionAsync ===");
                Console.WriteLine($"Found submission: ActivityId={submission.ActivityId}, StudentId={submission.StudentId}");
                Console.WriteLine($"SubmissionContent: {(string.IsNullOrEmpty(submission.SubmissionContent) ? "NULL/EMPTY" : $"Length={submission.SubmissionContent.Length}, Preview={submission.SubmissionContent.Substring(0, Math.Min(50, submission.SubmissionContent.Length))}...")}");
            }
            else
            {
                Console.WriteLine($"=== GetSubmissionAsync ===");
                Console.WriteLine($"No submission found for ActivityId={activityId}, StudentId={studentId}");
            }
            
            return submission;
        }

        public async Task SaveSubmissionAsync(ActivitySubmissionModel submission)
        {
            // Check if submission exists
            var existing = await GetSubmissionAsync(submission.ActivityId, submission.StudentId);

            // Debug: Log submission content
            Console.WriteLine($"=== SaveSubmissionAsync ===");
            Console.WriteLine($"ActivityId: {submission.ActivityId}, StudentId: {submission.StudentId}");
            Console.WriteLine($"SubmissionContent: {(string.IsNullOrEmpty(submission.SubmissionContent) ? "NULL/EMPTY" : $"Length={submission.SubmissionContent.Length}, Preview={submission.SubmissionContent.Substring(0, Math.Min(50, submission.SubmissionContent.Length))}...")}");

            if (existing == null)
            {
                // Create new submission
                submission.CreatedAt = DateTime.UtcNow;
                Console.WriteLine($"Creating new submission with SubmissionContent");
                await _client.From<ActivitySubmissionModel>().Insert(submission);
                Console.WriteLine($"New submission inserted");
            }
            else
            {
                // Update existing submission
                existing.Score = submission.Score;
                existing.SubmissionStatus = submission.SubmissionStatus ?? existing.SubmissionStatus;
                existing.Feedback = submission.Feedback ?? existing.Feedback;
                // Update submission content if provided (allow students to resubmit)
                if (!string.IsNullOrWhiteSpace(submission.SubmissionContent))
                {
                    existing.SubmissionContent = submission.SubmissionContent;
                    Console.WriteLine($"Updating existing submission with new SubmissionContent");
                }
                else
                {
                    Console.WriteLine($"WARNING: SubmissionContent is null/empty, not updating");
                }
                await existing.Update<ActivitySubmissionModel>();
                Console.WriteLine($"Existing submission updated");
            }
        }
    }
}
