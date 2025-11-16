using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public class UserWithRoleViewModel
    {
        public SupabaseUserNew User { get; set; }
        public List<RoleModel> Roles { get; set; } = new List<RoleModel>();

        /// <summary>
        /// Get comma-separated role names for this user
        /// </summary>
        public string GetRoleNames()
        {
            if (Roles == null || Roles.Count == 0)
                return "No Role";

            var roleNames = new List<string>();
            foreach (var role in Roles)
            {
                if (!string.IsNullOrWhiteSpace(role.RoleName))
                    roleNames.Add(role.RoleName);
            }

            return roleNames.Count > 0 ? string.Join(", ", roleNames) : "No Role";
        }
    }
}
