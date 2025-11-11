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
    /// Repository implementation for Activity entity operations.
    /// Handles activity/assignment management using Supabase queries.
    /// </summary>
    public class ActivityRepository : IActivityRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public ActivityRepository(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public async Task<List<Activity>> GetAllActivitiesAsync()
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<Activity>().Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching all activities: {ex.Message}");
                return new List<Activity>();
            }
        }

        public async Task<Activity> GetActivityByIdAsync(int activityId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<Activity>()
                    .Where(x => x.id == activityId)
                    .Single();
                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching activity {activityId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Activity>> GetActivitiesByCourseAsync(int courseId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<Activity>()
                    .Where(x => x.courseId == courseId && !x.isArchived)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching activities for course {courseId}: {ex.Message}");
                return new List<Activity>();
            }
        }

        public async Task<List<Activity>> GetAllActivitiesByCourseAsync(int courseId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<Activity>()
                    .Where(x => x.courseId == courseId)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching all activities for course {courseId}: {ex.Message}");
                return new List<Activity>();
            }
        }

        public async Task<List<Activity>> GetActivitiesByInstructorAsync(int teacherId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                
                // First get all courses taught by this instructor
                var coursesResponse = await client.From<Course>()
                    .Where(x => x.instructor == teacherId)
                    .Get();
                
                if (!coursesResponse.Models.Any())
                    return new List<Activity>();
                
                var courseIds = coursesResponse.Models.Select(c => c.id).ToList();
                
                // Get all activities in those courses
                var activities = new List<Activity>();
                foreach (var courseId in courseIds)
                {
                    var activitiesResponse = await client.From<Activity>()
                        .Where(x => x.courseId == courseId)
                        .Get();
                    activities.AddRange(activitiesResponse.Models);
                }
                
                return activities;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching activities for instructor {teacherId}: {ex.Message}");
                return new List<Activity>();
            }
        }

        public async Task<int> GetActivityCountByCourseAsync(int courseId)
        {
            try
            {
                var activities = await GetActivitiesByCourseAsync(courseId);
                return activities.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error counting activities for course {courseId}: {ex.Message}");
                return 0;
            }
        }

        public async Task<Activity> CreateActivityAsync(Activity activity)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<Activity>()
                    .Insert(new List<Activity> { activity });
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating activity: {ex.Message}");
                return null;
            }
        }

        public async Task<Activity> UpdateActivityAsync(Activity activity)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<Activity>()
                    .Update(activity);
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating activity: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ArchiveActivityAsync(int activityId)
        {
            try
            {
                var activity = await GetActivityByIdAsync(activityId);
                if (activity == null) return false;
                
                activity.isArchived = true;
                activity.archivedAt = DateTime.UtcNow;
                
                var updatedActivity = await UpdateActivityAsync(activity);
                return updatedActivity != null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error archiving activity {activityId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Activity>> GetArchivedActivitiesByCourseAsync(int courseId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var response = await client.From<Activity>()
                    .Where(x => x.courseId == courseId && x.isArchived)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching archived activities for course {courseId}: {ex.Message}");
                return new List<Activity>();
            }
        }

        public async Task<List<Activity>> GetUpcomingActivitiesByCourseAsync(int courseId)
        {
            try
            {
                await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
                var client = AsiBasecodeDBContext.SupabaseClient;
                var now = DateTime.UtcNow;
                
                var response = await client.From<Activity>()
                    .Where(x => x.courseId == courseId && !x.isArchived && x.dueDate > now)
                    .Get();
                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching upcoming activities for course {courseId}: {ex.Message}");
                return new List<Activity>();
            }
        }
    }
}
