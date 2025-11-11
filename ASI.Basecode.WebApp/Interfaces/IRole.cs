namespace ASI.Basecode.WebApp.Interfaces
{
    /// <summary>
    /// Interface for Role model.
    /// </summary>
    public interface IRole
    {
        int id { get; set; }
        string roleName { get; set; }
        int profileId { get; set; }
        string description { get; set; }
    }
}
