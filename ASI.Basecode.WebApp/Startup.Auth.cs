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
            // Skip authentication/authorization wiring entirely for UI work
            this._services.AddMvc();
        }
    }
}
