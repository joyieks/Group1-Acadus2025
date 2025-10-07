using Microsoft.Extensions.Configuration;
using Supabase;
using System.Threading.Tasks;

namespace ASI.Basecode.Data
{
    public partial class AsiBasecodeDBContext
    {
        private static Client _supabaseClient;

        // Initialize Supabase manually (not through constructor)
        public static async Task InitializeSupabaseAsync(IConfiguration configuration)
        {
            if (_supabaseClient != null)
                return;

            var url = configuration["Supabase:Url"];
            var key = configuration["Supabase:AnonKey"];

            _supabaseClient = new Client(url, key);
            await _supabaseClient.InitializeAsync();
        }

        // Gives access to the Supabase client
        public static Client SupabaseClient => _supabaseClient;
    }
}
