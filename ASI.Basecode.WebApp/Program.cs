using System.IO;
using ASI.Basecode.WebApp;
using ASI.Basecode.WebApp.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
});

// Ensure wwwroot directory exists
Directory.CreateDirectory(builder.Environment.WebRootPath);

builder.Configuration.AddJsonFile("appsettings.json",
    optional: true,
    reloadOnChange: true);

builder.WebHost.UseIISIntegration();

builder.Logging
    .AddConfiguration(builder.Configuration.GetLoggingSection())
    .AddConsole()
    .AddDebug();

var configurer = new StartupConfigurer(builder.Configuration);
configurer.ConfigureServices(builder.Services);

var app = builder.Build();

configurer.ConfigureApp(app, app.Environment);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Index}/{id?}");
app.MapControllers();
app.MapRazorPages();

app.Run();
