namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Represents emergency contact information for a user.
    /// </summary>
    public class EmergencyContact
    {
        /// <summary>
        /// Primary key identifier for the emergency contact.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Emergency contact's first name.
        /// </summary>
        public string firstName { get; set; }

        /// <summary>
        /// Emergency contact's last name.
        /// </summary>
        public string lastName { get; set; }

        /// <summary>
        /// Emergency contact's middle name (nullable).
        /// </summary>
        public string middleName { get; set; }

        /// <summary>
        /// Emergency contact's name suffix (e.g., Jr., Sr.) (nullable).
        /// </summary>
        public string suffix { get; set; }

        /// <summary>
        /// Emergency contact's phone number.
        /// </summary>
        public string contactNumber { get; set; }

        /// <summary>
        /// Relationship to the user (e.g., Parent, Sibling, Spouse, Friend).
        /// </summary>
        public string relationship { get; set; }
    }
}
