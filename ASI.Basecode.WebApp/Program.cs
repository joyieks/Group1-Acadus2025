using ASI.Basecode.Data;
using ASI.Basecode.WebApp;
using ASI.Basecode.WebApp.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
});

// Ensure wwwroot directory exists
Directory.CreateDirectory(builder.Environment.WebRootPath);

// Load appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.WebHost.UseIISIntegration();

// Configure logging
builder.Logging
    .AddConfiguration(builder.Configuration.GetLoggingSection())
    .AddConsole()
    .AddDebug();

// Add MVC support
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register your DbContext (internally supports Supabase)
builder.Services.AddDbContext<AsiBasecodeDBContext>(options =>
{
    // DbContext will be configured for Supabase or other providers as needed
});

// Register IConfiguration so your context's constructor can read appsettings.json
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var configurer = new StartupConfigurer(builder.Configuration);
configurer.ConfigureServices(builder.Services);

var app = builder.Build();

configurer.ConfigureApp(app, app.Environment);

// Use session middleware
app.UseSession();

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
