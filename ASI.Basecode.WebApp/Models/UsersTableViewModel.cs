using System.Collections.Generic;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.WebApp.Models
{
    public class UsersTableViewModel
    {
        /// <summary>
        /// All users (students + instructors + others)
        /// </summary>
        public List<SupabaseUserNew> AllUsers { get; set; } = new List<SupabaseUserNew>();

        /// <summary>
        /// Only students (roleId = 1)
        /// </summary>
        public List<SupabaseUserNew> Students { get; set; } = new List<SupabaseUserNew>();

        /// <summary>
        /// Only instructors (roleId = 2)
        /// </summary>
        public List<SupabaseUserNew> Instructors { get; set; } = new List<SupabaseUserNew>();

        /// <summary>
        /// Currently displayed users based on active tab
        /// </summary>
        public List<SupabaseUserNew> DisplayedUsers { get; set; } = new List<SupabaseUserNew>();

        /// <summary>
        /// Search term (if any) for retaining in the search input
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Displayed users including resolved roles (populated by controller)
        /// </summary>
        public List<UserWithRoleViewModel> DisplayedUsersWithRoles { get; set; } = new List<UserWithRoleViewModel>();

        /// <summary>
        /// Active tab: "all", "students", or "instructors"
        /// </summary>
        public string ActiveTab { get; set; } = "all";

        /// <summary>
        /// Active status filter: "all", "active", or "inactive"
        /// </summary>
        public string ActiveStatus { get; set; } = "all";

        /// <summary>
        /// Total counts for KPI cards
        /// </summary>
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }

        /// <summary>
        /// Helper method to get display name from user
        /// </summary>
        public static string GetFullName(SupabaseUserNew user)
        {
            if (user == null) return "Unknown";

            var parts = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(user.FirstName))
                parts.Add(user.FirstName);
            
            if (!string.IsNullOrWhiteSpace(user.MiddleName))
                parts.Add(user.MiddleName);
            
            if (!string.IsNullOrWhiteSpace(user.LastName))
                parts.Add(user.LastName);

            if (parts.Count == 0)
                return "Unknown";

            return string.Join(" ", parts);
        }

        /// <summary>
        /// Helper method to get role display name from roleId
        /// </summary>
        public static string GetRoleName(string userTypeId)
        {
            return userTypeId == "1" ? "Student" :
                   userTypeId == "2" ? "Instructor" :
                   "Unknown";
        }
    }
}
