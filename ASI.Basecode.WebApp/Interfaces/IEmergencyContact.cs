namespace ASI.Basecode.WebApp.Interfaces
{
    /// <summary>
    /// Interface for EmergencyContact model.
    /// </summary>
    public interface IEmergencyContact
    {
        int id { get; set; }
        string firstName { get; set; }
        string lastName { get; set; }
        string middleName { get; set; }
        string suffix { get; set; }
        string contactNumber { get; set; }
        string relationship { get; set; }
    }
}
