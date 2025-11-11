namespace ASI.Basecode.WebApp.Interfaces
{
    /// <summary>
    /// Interface for Program model.
    /// </summary>
    public interface IProgram
    {
        int id { get; set; }
        string programName { get; set; }
        string programCode { get; set; }
        int departmentId { get; set; }
    }
}
