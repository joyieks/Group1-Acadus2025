namespace ASI.Basecode.WebApp.Interfaces
{
    /// <summary>
    /// Interface for CourseEnrollment model.
    /// </summary>
    public interface ICourseEnrollment
    {
        int id { get; set; }
        int courseId { get; set; }
        int userId { get; set; }
        string enrollmentStatus { get; set; }
    }
}
