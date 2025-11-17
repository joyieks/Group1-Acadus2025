using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Models
{
    public class CourseGradebookViewModel
    {
        public long CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public long SemesterInfo { get; set; }
        public string CardColor { get; set; }

        public List<StudentGradeItem> Students { get; set; } = new();
        public List<ActivityGradeItem> Activities { get; set; } = new();

        public class StudentGradeItem
        {
            public string StudentId { get; set; }
            public string Name { get; set; }
            public double AveragePercentage { get; set; }
            public string GradeLetter { get; set; }
        }

        public class ActivityGradeItem
        {
            public int ActivityId { get; set; }
            public string ActivityName { get; set; }
            public int RawScore { get; set; }
            public int MaxScore { get; set; }
        }
        public StudentGradeDetail SelectedStudentDetail { get; set; }

        public class StudentGradeDetail
        {
            
            public string StudentId { get; set; }
            public string Name { get; set; }
            public double TotalRawScore { get; set; }
            public double TotalMaxScore { get; set; }
            public double Percentage => TotalMaxScore > 0 ? Math.Round((TotalRawScore / TotalMaxScore) * 100, 1) : 0;
            
            public List<ActivityGradeItem> Activities { get; set; } = new();
        }

    }

}
