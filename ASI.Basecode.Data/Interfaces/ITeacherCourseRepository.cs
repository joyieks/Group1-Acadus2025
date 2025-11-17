using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ASI.Basecode.Data.Models.CourseGradebookViewModel;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ITeacherCourseRepository
    {
        Task<CourseGradebookViewModel> GetCourseGradebookAsync(int courseId);

        Task<List<ActivityModel>> GetActivitiesByCourseIdAsync(long courseId);
        Task<List<ActivitySubmissionModel>> GetSubmissionsByStudentAndCourseAsync(string studentId, long courseId);

        Task<SupabaseUserNew> GetUserByUserTypeIdAsync(string userTypeId);

        Task<StudentGradeDetail> GetStudentGradeDetailAsync(string studentId, int courseId);

        Task<bool> UpdateActivityScoreAsync(string studentId, int activityId, int newScore);

    }
}
