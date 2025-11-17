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
            // REMOVED DUPLICATE: Authentication is already configured in Startup.cs ConfigureServices()
            // The AddAuthentication().AddCookie() was causing "Scheme already exists: Cookies" error

            // Authorization policies are already configured in Startup.cs, so this method can be empty
            // or removed entirely. Keeping it empty for now to avoid breaking changes.

            // Note: If you want to add additional authorization logic,
            // it should NOT include AddAuthentication() or AddCookie() calls.
        }
    }
}
