namespace ASI.Basecode.WebApp.Interfaces
{
    /// <summary>
    /// Interface for Address model.
    /// </summary>
    public interface IAddress
    {
        int id { get; set; }
        string house_number { get; set; }
        string street_name { get; set; }
        string subdivision { get; set; }
        string barangay { get; set; }
        string city { get; set; }
        string province { get; set; }
        string zipcode { get; set; }
    }
}
