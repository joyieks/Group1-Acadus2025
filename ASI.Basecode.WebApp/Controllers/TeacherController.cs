using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ASI.Basecode.WebApp.Models;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Controller for teacher-related actions and dashboard statistics.
    /// </summary>
    public class TeacherController : Controller
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherController"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        public TeacherController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Displays the teacher dashboard with statistics.
        /// </summary>
        /// <returns>The dashboard view.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);
            var client = AsiBasecodeDBContext.SupabaseClient;

            // TODO: Replace with actual teacher ID from authentication
            int teacherId = 1; // Example integer ID

            // Get total activities for the teacher
            var activitiesResponse = await client.From<ActivityModel>()
                .Filter("teacher_id", Supabase.Postgrest.Constants.Operator.Equals, teacherId)
                .Get();
            var activities = activitiesResponse.Models;
            int totalActivities = activities.Count;

            // Get graded activities for the teacher
            int gradedActivities = activities.Count(a => a.IsGraded);

            // Get total courses handled by the teacher
            var coursesResponse = await client.From<CourseModel>()
                .Filter("teacher_id", Supabase.Postgrest.Constants.Operator.Equals, teacherId)
                .Get();
            var courses = coursesResponse.Models;
            int totalCoursesHandled = courses.Count;

            // TODO: Implement calendar events retrieval if needed
            var calendarEvents = new List<string>();

            var model = new TeacherDashboardViewModel
            {
                TotalActivities = totalActivities,
                GradedActivities = gradedActivities,
                TotalCoursesHandled = totalCoursesHandled,
                CalendarEvents = calendarEvents
            };
            return View(model);
        }

        /// <summary>
        /// Displays the teacher's activities view.
        /// </summary>
        /// <returns>The activities view.</returns>
        [HttpGet]
        public IActionResult Activities()
        {
            return View();
        }

        /// <summary>
        /// Displays the teacher's grades view.
        /// </summary>
        /// <returns>The grades view.</returns>
        [HttpGet]
        public IActionResult Grades()
        {
            return View();
        }
    }
}



