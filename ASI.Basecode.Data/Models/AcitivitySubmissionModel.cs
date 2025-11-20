using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace ASI.Basecode.Data.Models
{
    [Table("activity_submission")]
    public class ActivitySubmissionModel : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("activityId")]
        public int ActivityId { get; set; }

        [Column("studentId")]
        public string StudentId { get; set; } 

        [Column("score")]
        public int Score { get; set; }


        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("submissionStatus")]
        public string SubmissionStatus { get; set; }

        [Column("feedback")]
        public string Feedback { get; set; }

        [Column("submissionContent")]
        public string SubmissionContent { get; set; }
        
        // Alternative column name mapping (in case database uses different casing)
        // If the above doesn't work, try: [Column("submission_content")]

    }
}
