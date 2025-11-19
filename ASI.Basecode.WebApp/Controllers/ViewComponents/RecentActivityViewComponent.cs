using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.Services.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers.ViewComponents
{
    /// <summary>
    /// View Component for rendering a list of recent activities in the Teacher dashboard.
    /// This component displays a styled list similar to the Admin dashboard's recent activity section.
    /// </summary>
    public class RecentActivityViewComponent : ViewComponent
    {
        private readonly IAuditLogService _auditLogService;

        public RecentActivityViewComponent(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Invokes the Recent Activity View Component.
        /// </summary>
        /// <returns>An IViewComponentResult containing the rendered recent activity list view.</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                // Get current user ID
                var currentUserId = ViewContext.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) 
                    ?? ViewContext.HttpContext.User.FindFirstValue("sub");

                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    return View(new System.Collections.Generic.List<ASI.Basecode.Data.Models.AuditLogModel>());
                }

                // Get recent audit logs for the current teacher (limit to 10 most recent)
                var auditLogs = await _auditLogService.GetRecentLogsByUserAsync(currentUserId, limit: 10);

                return View(auditLogs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading recent activities: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return View(new System.Collections.Generic.List<ASI.Basecode.Data.Models.AuditLogModel>());
            }
        }
    }
}
