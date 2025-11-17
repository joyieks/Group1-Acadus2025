using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ASI.Basecode.Data.Models.CourseGradebookViewModel;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITeacherCourseService
    {
        Task<CourseGradebookViewModel> GetCourseGradebookAsync(int courseId);
        Task<StudentGradeDetail> GetStudentGradeDetailAsync(string studentId, int courseId);

        Task<bool> UpdateActivityScoreAsync(string studentId, int activityId, int newScore);
    }
}
