using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ASI.Basecode.WebApp
{
    // Other services configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configures the other services.
        /// </summary>
        private void ConfigureOtherServices()
        {
            // Framework
            this._services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            this._services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // If no DB configured, use in-memory provider so frontend runs
            // Temporarily disabled to avoid missing EF Core InMemory package errors
            // this._services.AddDbContext<AsiBasecodeDBContext>(options =>
            // {
            //     options.UseInMemoryDatabase("AsiBasecodeDev");
            // });

            /*
             * Temporarily disabled backend service wiring while focusing on frontend only.
             * Uncomment when backend implementations are ready.
             *
             * // Common
             * this._services.AddScoped<TokenProvider>();
             * this._services.TryAddSingleton<TokenProviderOptionsFactory>();
             * this._services.TryAddSingleton<TokenValidationParametersFactory>();
             * this._services.AddScoped<IUnitOfWork, UnitOfWork>();
             *
             * // Services
             * this._services.TryAddSingleton<TokenValidationParametersFactory>();
             * this._services.AddScoped<IUserService, UserService>();
             *
             * // Repositories
             * this._services.AddScoped<IUserRepository, UserRepository>();
             *
             * // Manager Class
             * this._services.AddScoped<SignInManager>();
             */

            this._services.AddHttpClient();
        }
    }
}
