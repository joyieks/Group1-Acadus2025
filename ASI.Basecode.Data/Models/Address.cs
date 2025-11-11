using Supabase.Postgrest.Models;

namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Represents a physical address (for users).
    /// </summary>
    public class Address : BaseModel
    {
        /// <summary>
        /// Primary key identifier for the address.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// House or building number.
        /// </summary>
        public string house_number { get; set; }

        /// <summary>
        /// Street name.
        /// </summary>
        public string street_name { get; set; }

        /// <summary>
        /// Subdivision or complex name (nullable).
        /// </summary>
        public string subdivision { get; set; }

        /// <summary>
        /// Barangay (village/district).
        /// </summary>
        public string barangay { get; set; }

        /// <summary>
        /// City.
        /// </summary>
        public string city { get; set; }

        /// <summary>
        /// Province or state.
        /// </summary>
        public string province { get; set; }

        /// <summary>
        /// Postal/zip code.
        /// </summary>
        public string zipcode { get; set; }
    }
}
