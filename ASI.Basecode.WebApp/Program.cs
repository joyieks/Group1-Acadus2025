using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp;
using ASI.Basecode.WebApp.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

// Set up SSL bypass for development BEFORE creating the builder
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
#pragma warning disable SYSLIB0014
    System.Net.ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls;
#pragma warning restore SYSLIB0014
    Console.WriteLine("Global SSL certificate validation bypassed for development");
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
});

// Ensure wwwroot directory exists
Directory.CreateDirectory(builder.Environment.WebRootPath);

// Load appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// Load environment-specific appsettings
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Load local development overrides (if they exist)
if (builder.Environment.EnvironmentName == "Development")
{
    builder.Configuration.AddJsonFile("appsettings.Development.local.json", optional: true, reloadOnChange: true);
}

// Add environment variables
builder.Configuration.AddEnvironmentVariables();

// Debug configuration loading
if (builder.Environment.EnvironmentName == "Development")
{
    var supabaseUrl = builder.Configuration["Supabase:Url"];
    var supabaseKey = builder.Configuration["Supabase:AnonKey"];
    var serviceKey = builder.Configuration["Supabase:ServiceRoleKey"];
    
    Console.WriteLine("=== CONFIGURATION DEBUG ===");
    Console.WriteLine($"Supabase URL: {supabaseUrl}");
    Console.WriteLine($"Supabase Anon Key: {supabaseKey?.Substring(0, Math.Min(20, supabaseKey?.Length ?? 0))}...");
    Console.WriteLine($"Service Role Key: {serviceKey?.Substring(0, Math.Min(20, serviceKey?.Length ?? 0))}...");
    Console.WriteLine("==========================");
}

builder.WebHost.UseIISIntegration();

// Configure logging
builder.Logging
    .AddConfiguration(builder.Configuration.GetLoggingSection())
    .AddConsole()
    .AddDebug();

// Register your DbContext (internally supports Supabase)
builder.Services.AddDbContext<AsiBasecodeDBContext>();

// Register IConfiguration so your context's constructor can read appsettings.json
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Configure HttpClient with SSL bypass for development
if (builder.Environment.EnvironmentName == "Development")
{
    builder.Services.AddHttpClient("SupabaseClient", client =>
    {
        // Configure the HttpClient
    }).ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new System.Net.Http.HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        return handler;
    });
}

// Register Supabase Client as a singleton
builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var url = configuration["Supabase:Url"];
    var key = configuration["Supabase:AnonKey"];

    if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
        throw new InvalidOperationException("Supabase configuration (Url or AnonKey) is missing in appsettings.json.");

    var client = new Supabase.Client(url, key);
    client.InitializeAsync().Wait(); // sync init at startup
    return client;
});

builder.Services.AddScoped<IStudentCourseService, StudentCourseService>();
builder.Services.AddScoped<IStudentCourseRepository, StudentCourseRepository>();


builder.Services.AddScoped<ITeacherCourseService, TeacherCourseService>();
builder.Services.AddScoped<ITeacherCourseRepository, TeacherCourseRepository>();

builder.Services.AddScoped<ASI.Basecode.Services.Interfaces.IAuditLogService, ASI.Basecode.Services.Services.AuditLogService>();

var configurer = new StartupConfigurer(builder.Configuration);
configurer.ConfigureServices(builder.Services);

var app = builder.Build();

configurer.ConfigureApp(app, app.Environment);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "fallback",
    pattern: "{*url}",
defaults: new { controller = "Home", action = "Index" });

app.MapControllers();
app.MapRazorPages();

app.Run();
