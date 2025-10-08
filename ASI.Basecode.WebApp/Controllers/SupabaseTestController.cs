using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    public class SupabaseTestController : Controller
    {
        private readonly IConfiguration _configuration;

        public SupabaseTestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            // Initialize Supabase connection
            await AsiBasecodeDBContext.InitializeSupabaseAsync(_configuration);

            var client = AsiBasecodeDBContext.SupabaseClient;

            // Fetch data from Supabase table "Users"
            var response = await client.From<SupabaseUser>().Get();

            if (response.Models != null && response.Models.Any())
            {
                ViewBag.Message = "✅ Successfully connected and retrieved data from Supabase!";
                return View(response.Models);
            }
            else
            {
                ViewBag.Message = "⚠️ Connected but no records found in 'Users' table.";
                return View(Enumerable.Empty<SupabaseUser>());
            }
        }
    }
}
