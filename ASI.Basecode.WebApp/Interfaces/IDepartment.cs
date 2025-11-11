namespace ASI.Basecode.WebApp.Interfaces
{
    /// <summary>
    /// Interface for Department model.
    /// </summary>
    public interface IDepartment
    {
        int id { get; set; }
        string departmentName { get; set; }
        string departmentCode { get; set; }
    }
}
