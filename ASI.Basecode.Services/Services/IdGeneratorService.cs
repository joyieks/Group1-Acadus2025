using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Generates unique student and teacher IDs in the format:
    /// STU-YEARMONTHINDEX (e.g., STU-202511001)
    /// FAC-YEARMONTHINDEX (e.g., FAC-202511001)
    /// ADM-YEARMONTHINDEX (e.g., ADM-202511001)
    /// </summary>
    public class IdGeneratorService
    {
        private readonly ISupabaseAuthService _supabaseAuthService;

        public IdGeneratorService(ISupabaseAuthService supabaseAuthService)
    {
            _supabaseAuthService = supabaseAuthService;
        }

  /// <summary>
        /// Generates a unique student ID
        /// Format: STU-YEARMONTHINDEX
      /// Example: STU-202511001 (2025, November, 1st student)
        /// </summary>
        public async Task<string> GenerateStudentIdAsync()
    {
            return await GenerateIdAsync("STU");
     }

     /// <summary>
    /// Generates a unique teacher/faculty ID
        /// Format: FAC-YEARMONTHINDEX
        /// Example: FAC-202511001 (2025, November, 1st faculty)
        /// </summary>
        public async Task<string> GenerateTeacherIdAsync()
        {
            return await GenerateIdAsync("FAC");
   }

        /// <summary>
        /// Generates a unique admin ID
        /// Format: ADM-YEARMONTHINDEX
        /// Example: ADM-202511001 (2025, November, 1st admin)
        /// </summary>
public async Task<string> GenerateAdminIdAsync()
        {
            return await GenerateIdAsync("ADM");
        }

        /// <summary>
    /// Core ID generation logic
        /// </summary>
        private async Task<string> GenerateIdAsync(string prefix)
        {
            try
 {
    var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();
  
                  var now = DateTime.UtcNow;
                  var year = now.Year;
                  var month = now.Month.ToString("D2"); 
  
   
                var yearMonth = $"{year}{month}";
                var searchPattern = $"{prefix}-{yearMonth}";

                Console.WriteLine($"Generating {prefix} ID for {year}-{month}");
                Console.WriteLine($"Search pattern: {searchPattern}");

         // Query users table to find existing IDs with this pattern
                 var usersQuery = await client
                .From<Data.Models.SupabaseUserNew>()
                .Get();

                 var users = usersQuery?.Models ?? new List<SupabaseUserNew>();

    
                var matchingIds = users
                .Where(u => !string.IsNullOrEmpty(u.UserDisplayId) && 
                 u.UserDisplayId.StartsWith(searchPattern))
                .Select(u => u.UserDisplayId)
                .ToList();

                Console.WriteLine($"Found {matchingIds.Count} existing IDs with pattern {searchPattern}");

      
                 int maxIndex = 0;
                foreach (var id in matchingIds)
                  {
   
                 var parts = id.Split('-');
                    if (parts.Length == 2 && parts[1].Length >= 9) // YYYYMMNNN
                     {
                         var indexStr = parts[1].Substring(6); // Get last 3+ digits
                            if (int.TryParse(indexStr, out int index))
                            {
                                 if (index > maxIndex)
                                {
                                     maxIndex = index;
                                         }
                                  }
                              }
                         }

    
                       var newIndex = maxIndex + 1;
                        var newId = $"{prefix}-{yearMonth}{newIndex:D3}"; 

                        Console.WriteLine($"Generated new ID: {newId}");
                        Console.WriteLine($"  - Prefix: {prefix}");
                        Console.WriteLine($"  - Year-Month: {yearMonth}");
                        Console.WriteLine($"  - Index: {newIndex}");

                    return newId;
                          }
                    catch (Exception ex)
                      {
                     Console.WriteLine($"Error generating {prefix} ID: {ex.Message}");
                        throw new Exception($"Failed to generate {prefix} ID", ex);
                         }
              }

        /// <summary>
  /// Validates an ID format
    /// </summary>
     public bool ValidateIdFormat(string id, string expectedPrefix)
     {
         if (string.IsNullOrEmpty(id))
   return false;

          
            var parts = id.Split('-');
            if (parts.Length != 2)
              return false;

     if (parts[0] != expectedPrefix)
 return false;

     if (parts[1].Length < 9) 
                return false;

     
            var yearMonth = parts[1].Substring(0, 6);
   if (!int.TryParse(yearMonth, out _))
   return false;

        
         var index = parts[1].Substring(6);
 if (!int.TryParse(index, out _))
       return false;

      return true;
        }

        /// <summary>
        /// Parses an ID to extract components
        /// </summary>
   public (string prefix, int year, int month, int index) ParseId(string id)
  {
            var parts = id.Split('-');
  if (parts.Length != 2)
    throw new ArgumentException("Invalid ID format");

   var prefix = parts[0];
            var yearMonth = parts[1].Substring(0, 6);
   var indexStr = parts[1].Substring(6);

   var year = int.Parse(yearMonth.Substring(0, 4));
    var month = int.Parse(yearMonth.Substring(4, 2));
            var index = int.Parse(indexStr);

            return (prefix, year, month, index);
  }
    }
}
