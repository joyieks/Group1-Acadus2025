using System;
using System.Collections.Generic;

namespace ASI.Basecode.Services.ServiceModels
{
    public class TeacherCourseModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public string SemesterInfo { get; set; }
        public string CardColor { get; set; }
        public int Id { get; set; }

        public List<TeacherActivityModel> Activities { get; set; } = new List<TeacherActivityModel>();
        public List<TeacherStudentModel> Students { get; set; } = new List<TeacherStudentModel>();
        public List<TeacherActivitySubmissionModel> Submissions { get; set; } = new List<TeacherActivitySubmissionModel>();
    }
}
