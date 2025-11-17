using System.Collections.Generic;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.WebApp.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalCourses { get; set; }
        public List<AuditLogModel> RecentActivities { get; set; } = new List<AuditLogModel>();
    }
}