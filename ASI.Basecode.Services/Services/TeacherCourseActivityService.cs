using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Services
{
    public class TeacherCourseActivityService : ITeacherCourseActivityService
    {
        private readonly ITeacherCourseActivityRepository _repo;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly ISupabaseAuthService _supabaseAuthService;

        public TeacherCourseActivityService(ITeacherCourseActivityRepository repo, ICourseService courseService, IUserService userService, ISupabaseAuthService supabaseAuthService)
        {
            _repo = repo;
            _courseService = courseService;
            _userService = userService;
            _supabaseAuthService = supabaseAuthService;
        }

        public async Task<TeacherCourseModel> LoadCourseActivityPageAsync(int courseId)
        {
            // Get course info
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
                throw new Exception($"Course with ID {courseId} not found");

            // Get activities, students, and submissions
            Console.WriteLine($"=== LoadCourseActivityPageAsync START ===");
            Console.WriteLine($"CourseId: {courseId}");
            
            var activities = await _repo.GetActivitiesByCourseAsync(courseId) ?? new List<ActivityModel>();
            Console.WriteLine($"Activities found: {activities.Count}");
            
            // Get students using the SAME approach as StudentTableViewComponent
            var students = new List<SupabaseUserNew>();
            try
            {
                var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();
                
                // Get all enrollments for this course (filter active in memory to avoid PostgREST issues)
                var enrollmentsResponse = await client
                    .From<EnrollmentModel>()
                    .Filter("course_id", Supabase.Postgrest.Constants.Operator.Equals, (long)courseId)
                    .Get();
                
                // Filter for active status in memory (check for "Active" enum value)
                var allEnrollments = enrollmentsResponse?.Models ?? new List<EnrollmentModel>();
                var enrollments = allEnrollments
                    .Where(e => !string.IsNullOrEmpty(e.Status) && 
                               (e.Status == "Active" || e.Status.Equals("active", StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                Console.WriteLine($"Found {enrollments.Count} active enrollments for course {courseId}");

                // Get all students (users with student role) - SAME as StudentTableViewComponent
                var allStudents = await _userService.GetStudentsAsync();
                Console.WriteLine($"Retrieved {allStudents.Count} students from UserService");

                // Map enrollments to students (SAME logic as StudentTableViewComponent)
                foreach (var enrollment in enrollments)
                {
                    var student = allStudents.FirstOrDefault(s => s.UserTypeId == enrollment.StudentId);
                    if (student != null)
                    {
                        students.Add(student);
                    }
                }

                Console.WriteLine($"Matched {students.Count} enrolled students for course {courseId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading enrolled students: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            
            var submissions = await _repo.GetSubmissionsByCourseAsync(courseId) ?? new List<ActivitySubmissionModel>();
            Console.WriteLine($"Submissions found: {submissions.Count}");
            foreach (var sub in submissions.Take(5))
            {
                Console.WriteLine($"  Submission: ActivityId={sub.ActivityId}, StudentId={sub.StudentId}, Score={sub.Score}, Status={sub.SubmissionStatus}");
            }

            var model = new TeacherCourseModel
            {
                CourseId = courseId,
                CourseCode = course.Code ?? "N/A",
                CourseTitle = course.Name ?? "Untitled Course",
                SemesterInfo = course.SemesterId?.ToString() ?? "No Semester",
                CardColor = "#E8F9E8",
                Id = (int)course.Id,

                Activities = activities.Select(a => new TeacherActivityModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    CourseId = a.CourseId,
                    MaxScore = a.maxScore,
                    DueDate = a.DueDate,
                    CreatedAt = a.CreatedAt,
                    IsVisible = !a.IsVisible, // Inverted: false = visible, true = hidden
                    InvisibleAt = a.InvisibleAt
                }).ToList(),

                Students = students.Select(s => new TeacherStudentModel
                {
                    Id = s.UserTypeId,
                    FirstName = s.FirstName,
                    MiddleName = s.MiddleName,
                    LastName = s.LastName,
                    Suffix = s.Suffix,
                    IsActive = s.IsActive ?? true
                }).ToList(),

                Submissions = submissions.Select(sub => new TeacherActivitySubmissionModel
                {
                    Id = sub.Id.ToString(),
                    ActivityId = sub.ActivityId,
                    StudentId = sub.StudentId,
                    Score = sub.Score,
                    SubmissionStatus = sub.SubmissionStatus,
                    CreatedAt = sub.CreatedAt,
                    Feedback = sub.Feedback,
                    SubmissionContent = sub.SubmissionContent
                }).ToList()
            };
            
            Console.WriteLine($"=== LoadCourseActivityPageAsync END ===");
            Console.WriteLine($"Returning model with {model.Students.Count} students, {model.Activities.Count} activities, {model.Submissions.Count} submissions");
            return model;
        }

        public async Task<TeacherActivityModel> GetActivityDetailsAsync(int activityId)
        {
            var activity = await _repo.GetActivityByIdAsync(activityId);
            if (activity == null)
                throw new Exception($"Activity with ID {activityId} not found");

            return new TeacherActivityModel
            {
                Id = activity.Id,
                Title = activity.Title,
                Description = activity.Description,
                CourseId = activity.CourseId,
                MaxScore = activity.maxScore,
                DueDate = activity.DueDate,
                CreatedAt = activity.CreatedAt,
                IsVisible = !activity.IsVisible, // Inverted: false = visible, true = hidden
                InvisibleAt = activity.InvisibleAt
            };
        }

        public async Task CreateActivityAsync(TeacherActivityModel model)
        {
            // In the UI: IsVisible = true means "visible to students", IsVisible = false means "hidden"
            // In the database: IsVisible = false means "visible to students", IsVisible = true means "hidden"
            // So we need to invert: if UI says visible (true), DB should be false
            var dbIsVisible = !model.IsVisible;
            
            Console.WriteLine($"=== CreateActivityAsync ===");
            Console.WriteLine($"UI IsVisible: {model.IsVisible} (true=visible, false=hidden)");
            Console.WriteLine($"DB IsVisible: {dbIsVisible} (false=visible, true=hidden)");
            
            var activity = new ActivityModel
            {
                Title = model.Title,
                Description = model.Description,
                CourseId = model.CourseId,
                maxScore = model.MaxScore,
                DueDate = model.DueDate,
                CreatedAt = DateTime.UtcNow,
                IsVisible = dbIsVisible, // Inverted: false = visible to students, true = hidden from students
                InvisibleAt = model.IsVisible ? (DateTime?)null : DateTime.UtcNow // Set invisible_at when hidden
            };

            await _repo.CreateActivityAsync(activity);
            Console.WriteLine($"Activity created with IsVisible={activity.IsVisible}");
        }

        public async Task UpdateActivityAsync(TeacherActivityModel model)
        {
            var activity = await _repo.GetActivityByIdAsync(model.Id);
            if (activity == null)
                throw new Exception($"Activity with ID {model.Id} not found");

            // In the UI: IsVisible = true means "visible to students", IsVisible = false means "hidden"
            // In the database: IsVisible = false means "visible to students", IsVisible = true means "hidden"
            var dbIsVisible = !model.IsVisible;
            
            Console.WriteLine($"=== UpdateActivityAsync ===");
            Console.WriteLine($"Activity ID: {model.Id}");
            Console.WriteLine($"UI IsVisible: {model.IsVisible} (true=visible, false=hidden)");
            Console.WriteLine($"DB IsVisible: {dbIsVisible} (false=visible, true=hidden)");

            activity.Title = model.Title;
            activity.Description = model.Description;
            activity.maxScore = model.MaxScore;
            activity.DueDate = model.DueDate;
            activity.IsVisible = dbIsVisible; // Inverted: false = visible to students, true = hidden from students
            activity.InvisibleAt = model.IsVisible ? (DateTime?)null : DateTime.UtcNow; // Set invisible_at when hidden

            await _repo.UpdateActivityAsync(activity);
            Console.WriteLine($"Activity updated with IsVisible={activity.IsVisible}");
        }

        public async Task DeleteActivityAsync(int activityId)
        {
            await _repo.DeleteActivityAsync(activityId);
        }

        public async Task GradeActivityAsync(TeacherActivitySubmissionModel submission)
        {
            // Check if feedback already exists - prevent updating if it does
            var existing = await _repo.GetSubmissionAsync(submission.ActivityId, submission.StudentId);
            if (existing != null && !string.IsNullOrWhiteSpace(existing.Feedback))
            {
                // Feedback already exists - only update score, not feedback
                Console.WriteLine($"=== GradeActivityAsync: Feedback already exists, only updating score ===");
                var submissionModel = new ActivitySubmissionModel
                {
                    ActivityId = submission.ActivityId,
                    StudentId = submission.StudentId,
                    Score = submission.Score,
                    SubmissionStatus = submission.SubmissionStatus ?? existing.SubmissionStatus ?? "Graded",
                    Feedback = existing.Feedback, // Keep existing feedback
                    SubmissionContent = existing.SubmissionContent, // Keep existing content
                    CreatedAt = existing.CreatedAt // Keep original creation date
                };
                await _repo.SaveSubmissionAsync(submissionModel);
            }
            else
            {
                // No feedback exists - allow setting new feedback
                var submissionModel = new ActivitySubmissionModel
                {
                    ActivityId = submission.ActivityId,
                    StudentId = submission.StudentId,
                    Score = submission.Score,
                    SubmissionStatus = submission.SubmissionStatus ?? "Graded",
                    Feedback = submission.Feedback,
                    SubmissionContent = existing?.SubmissionContent, // Preserve existing content if any
                    CreatedAt = existing?.CreatedAt ?? DateTime.UtcNow
                };

                await _repo.SaveSubmissionAsync(submissionModel);
            }
        }
    }
}
