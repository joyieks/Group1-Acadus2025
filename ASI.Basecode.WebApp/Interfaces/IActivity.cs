using System;

namespace ASI.Basecode.WebApp.Interfaces
{
    /// <summary>
    /// Interface for Activity model.
    /// </summary>
    public interface IActivity
    {
        int id { get; set; }
        int courseId { get; set; }
        string activityTitle { get; set; }
        string description { get; set; }
        bool isArchived { get; set; }
        DateTime? archivedAt { get; set; }
        DateTime? dueDate { get; set; }
        double maxScore { get; set; }
    }
}
