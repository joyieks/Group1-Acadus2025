namespace ASI.Basecode.WebApp.Interfaces
{
    /// <summary>
    /// Interface for ActivitySubmission model.
    /// </summary>
    public interface IActivitySubmission
    {
        int id { get; set; }
        int activityId { get; set; }
        int studentId { get; set; }
        double? score { get; set; }
        string submissionStatus { get; set; }
        string feedback { get; set; }
    }
}
