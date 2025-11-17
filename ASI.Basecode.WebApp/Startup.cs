using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Data;
using ASI.Basecode.WebApp.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text;

namespace ASI.Basecode.WebApp
{
    /// <summary>
    /// For configuring services on application startup.
    /// </summary>
    /// <remarks>
    /// <para>Method call sequence for instances of this class:</para>
    /// <para>1. constructor</para>
    /// <para>2. <see cref="ConfigureServices(IServiceCollection)"/></para>
    /// <para>3. (create <see cref="IApplicationBuilder"/> instance)</para>
    /// <para>4. <see cref="ConfigureApp(IApplicationBuilder, IWebHostEnvironment)"/></para>
    /// </remarks>
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        private IConfiguration Configuration { get; }

        private IApplicationBuilder _app;

        private IWebHostEnvironment _environment;

        private IServiceCollection _services;

        /// <summary>
        /// Initialize new <see cref="StartupConfigurer"/> instance using <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration"></param>
        public StartupConfigurer(IConfiguration configuration)
        {
            this.Configuration = configuration;

            PathManager.Setup(this.Configuration.GetSetupRootDirectoryPath());

            PasswordManager.SetUp(this.Configuration.GetSection("TokenAuthentication"));
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// Use this method to add services to the container.
        /// </summary>
        /// <param name="services">Services</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this._services = services;

            services.AddMemoryCache();
            services.AddControllersWithViews();
            services.AddRazorPages().AddRazorRuntimeCompilation();

            //Session
            services.AddSession(options =>
            {
                options.Cookie.Name = Const.Issuer;
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add Authentication with Cookie Scheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/Auth/AccessDenied";
                
                // ✅ FIX: Cookie configuration for development
                // Option 1: Session cookie (expires when browser closes)
                // Uncomment the following lines for development to prevent persistent login
                // options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Shorter expiration
                // options.Cookie.MaxAge = null; // Session cookie (expires when browser closes)
                
                // Option 2: Persistent cookie (current production setting)
                options.ExpireTimeSpan = TimeSpan.FromHours(8); // Cookie expires after 8 hours
                options.SlidingExpiration = true; // Renew cookie on each request
                
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                
                // ✅ NOTE: To force logout when stopping debugger during development,
                // you can either:
                // 1. Always click logout before stopping the debugger
                // 2. Use incognito/private browsing for testing
                // 3. Clear browser cookies manually
                // 4. Set Cookie.MaxAge = null above to use session cookies
            });

            // Add Authorization with Role-based policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("TeacherOnly", policy => policy.RequireRole("Teacher"));
                options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
                options.AddPolicy("TeacherOrAdmin", policy => policy.RequireRole("Teacher", "Admin"));
                options.AddPolicy("StudentOrTeacher", policy => policy.RequireRole("Student", "Teacher"));
            });

            // DI Services AutoMapper(Add Profile)
            this.ConfigureAutoMapper();

            // DI Services
            this.ConfigureOtherServices();

            // Authentication already configured above
            
            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = 1024 * 1024 * 100;
            });

            // Ensure wwwroot exists and configure static files
            var webRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            if (!Directory.Exists(webRootPath))
            {
                Directory.CreateDirectory(webRootPath);
            }
        }

        /// <summary>
        /// Configure application
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void ConfigureApp(IApplicationBuilder app, IWebHostEnvironment env)
        {
            this._app = app;
            this._environment = env;

            if (!this._environment.IsDevelopment())
            {
                this._app.UseHsts();
            }

            this.ConfigureLogger();

            this._app.UseStaticFiles();
            
            // Localization
            var options = this._app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            this._app.UseRequestLocalization(options.Value);

            this._app.UseSession();
            this._app.UseRouting();

            this._app.UseAuthentication();  
            this._app.UseAuthorization();   

        }
    }
}
