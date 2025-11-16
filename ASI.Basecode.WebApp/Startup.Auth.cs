using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;

namespace ASI.Basecode.WebApp
{
    // Authorization configuration
    internal partial class StartupConfigurer
    {
        // Removed unused fields to silence build warnings

        /// <summary>
        /// Configure authorization
        /// </summary>
        private void ConfigureAuthorization()
        {
            // Authentication is already configured in Startup.cs
            // This method is kept for backward compatibility but all auth setup is in Startup.cs
        }
    }
}
