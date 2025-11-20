using System;

namespace ASI.Basecode.Services.ServiceModels
{
    public class TeacherActivityModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long CourseId { get; set; }
        public int MaxScore { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsVisible { get; set; }
        public DateTime? InvisibleAt { get; set; }
    }
}
