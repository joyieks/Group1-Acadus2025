namespace ASI.Basecode.WebApp.Interfaces
{
    /// <summary>
    /// Interface for Course model.
    /// </summary>
    public interface ICourse
    {
        int id { get; set; }
        string courseCode { get; set; }
        string courseName { get; set; }
        string courseDesc { get; set; }
        int credits { get; set; }
        int semesterId { get; set; }
        int capacity { get; set; }
        int instructor { get; set; }
        string status { get; set; }
    }
}
