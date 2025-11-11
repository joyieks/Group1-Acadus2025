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

            // Database & Unit of Work
            this._services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Enable User Authentication via UserService
            this._services.AddScoped<IUserService, UserService>();
            this._services.AddScoped<IUserRepository, UserRepository>();
            this._services.AddScoped<SignInManager>();

            // Course, Activity, and Submission Repositories
            this._services.AddScoped<ICourseRepository, CourseRepository>();
            this._services.AddScoped<IActivityRepository, ActivityRepository>();
            this._services.AddScoped<IActivitySubmissionRepository, ActivitySubmissionRepository>();

            // Teacher Service for dashboard statistics
            this._services.AddScoped<ITeacherService, TeacherService>();

            this._services.AddHttpClient();
        }
    }
}
