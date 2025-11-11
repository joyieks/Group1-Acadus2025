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
    /// Repository implementation for ActivitySubmission entity operations.
    /// Handles student submissions and grading using Supabase queries.
    /// </summary>
    public class ActivitySubmissionRepository : IActivitySubmissionRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public ActivitySubmissionRepository(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public async Task<List<ActivitySubmission>> GetAllSubmissionsAsync()
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<ActivitySubmission>().Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching all submissions: {ex.Message}");
                return new List<ActivitySubmission>();
            }
        }

        public async Task<ActivitySubmission> GetSubmissionByIdAsync(int submissionId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<ActivitySubmission>()
                    .Where(x => x.id == submissionId)
                    .Single();
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching submission {submissionId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<ActivitySubmission>> GetSubmissionsByActivityAsync(int activityId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<ActivitySubmission>()
                    .Where(x => x.activityId == activityId)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching submissions for activity {activityId}: {ex.Message}");
                return new List<ActivitySubmission>();
            }
        }

        public async Task<List<ActivitySubmission>> GetSubmissionsByStudentAsync(int studentId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<ActivitySubmission>()
                    .Where(x => x.studentId == studentId)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching submissions for student {studentId}: {ex.Message}");
                return new List<ActivitySubmission>();
            }
        }

        public async Task<List<ActivitySubmission>> GetSubmissionsByStudentAndCourseAsync(int studentId, int courseId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                
                // Get all activities in the course
                var activitiesResponse = await client.From<Activity>()
                    .Where(x => x.courseId == courseId)
                    .Get();
                
                if (!activitiesResponse.Models.Any())
                    return new List<ActivitySubmission>();
                
                var activityIds = activitiesResponse.Models.Select(a => a.id).ToList();
                
                // Get all submissions by this student for those activities
                var submissions = new List<ActivitySubmission>();
                foreach (var activityId in activityIds)
                {
                    var submissionsResponse = await client.From<ActivitySubmission>()
                        .Where(x => x.activityId == activityId && x.studentId == studentId)
                        .Get();
                    submissions.AddRange(submissionsResponse.Models);
                }
                
                return submissions;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching submissions for student {studentId} in course {courseId}: {ex.Message}");
                return new List<ActivitySubmission>();
            }
        }

        public async Task<ActivitySubmission> GetSubmissionByActivityAndStudentAsync(int activityId, int studentId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<ActivitySubmission>()
                    .Where(x => x.activityId == activityId && x.studentId == studentId)
                    .Single();
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching submission for activity {activityId} and student {studentId}: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetSubmissionCountByActivityAsync(int activityId)
        {
            try
            {
                var submissions = await GetSubmissionsByActivityAsync(activityId);
                return submissions.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error counting submissions for activity {activityId}: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetGradedSubmissionCountByActivityAsync(int activityId)
        {
            try
            {
                var submissions = await GetSubmissionsByActivityAsync(activityId);
                return submissions.Count(s => s.submissionStatus == "Graded" || (!string.IsNullOrEmpty(s.submissionStatus) && s.score > 0));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error counting graded submissions for activity {activityId}: {ex.Message}");
                return 0;
            }
        }

        public async Task<ActivitySubmission> CreateSubmissionAsync(ActivitySubmission submission)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<ActivitySubmission>()
                    .Insert(new List<ActivitySubmission> { submission });
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating submission: {ex.Message}");
                return null;
            }
        }

        public async Task<ActivitySubmission> UpdateSubmissionAsync(ActivitySubmission submission)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<ActivitySubmission>()
                    .Update(submission);
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating submission: {ex.Message}");
                return null;
            }
        }

        public async Task<List<ActivitySubmission>> GetUngradedSubmissionsByActivityAsync(int activityId)
        {
            try
            {
                var submissions = await GetSubmissionsByActivityAsync(activityId);
                return submissions.Where(s => s.submissionStatus != "Graded").ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching ungraded submissions for activity {activityId}: {ex.Message}");
                return new List<ActivitySubmission>();
            }
        }

        public async Task<double> GetAverageScoreByActivityAsync(int activityId)
        {
            try
            {
                var submissions = await GetSubmissionsByActivityAsync(activityId);
                if (!submissions.Any()) return 0;
                
                var gradedSubmissions = submissions.Where(s => s.score.HasValue && s.score > 0).ToList();
                if (!gradedSubmissions.Any()) return 0;
                
                return gradedSubmissions.Average(s => s.score.Value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating average score for activity {activityId}: {ex.Message}");
                return 0;
            }
        }

        public async Task<double> GetAverageScoreByStudentAsync(int studentId)
        {
            try
            {
                var submissions = await GetSubmissionsByStudentAsync(studentId);
                if (!submissions.Any()) return 0;
                
                var gradedSubmissions = submissions.Where(s => s.score.HasValue && s.score > 0).ToList();
                if (!gradedSubmissions.Any()) return 0;
                
                return gradedSubmissions.Average(s => s.score.Value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating average score for student {studentId}: {ex.Message}");
                return 0;
            }
        }
    }
}
