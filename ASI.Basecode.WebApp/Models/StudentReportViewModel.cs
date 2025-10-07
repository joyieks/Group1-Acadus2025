using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public class StudentReportViewModel
    {
        public List<ReportItem> Reports { get; set; } = new();

        public class ReportItem
        {
            public string CourseCode { get; set; } = string.Empty;
            public string CourseTitle { get; set; } = string.Empty;
            public double MidtermGrade { get; set; }
            public double FinalGrade { get; set; }
        }
    }
}
