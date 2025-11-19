using System;

namespace ASI.Basecode.Services.ServiceModels
{
    public class TeacherActivitySubmissionModel
    {
        public string Id { get; set; }
        public int ActivityId { get; set; }
        public string StudentId { get; set; }
        public int Score { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SubmissionStatus { get; set; }
        public string Feedback { get; set; }
        public string SubmissionContent { get; set; }
    }
}
