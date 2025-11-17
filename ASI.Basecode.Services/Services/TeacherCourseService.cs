using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static ASI.Basecode.Data.Models.CourseGradebookViewModel;

namespace ASI.Basecode.Services.Services
{
    public class TeacherCourseService : ITeacherCourseService
    {

        private readonly ITeacherCourseRepository _teacherCourseRepository;

        public TeacherCourseService(ITeacherCourseRepository teacherCourseRepository)
        {
            _teacherCourseRepository = teacherCourseRepository;
        }

        public async Task<CourseGradebookViewModel> GetCourseGradebookAsync(int courseId)
        {
            return await _teacherCourseRepository.GetCourseGradebookAsync(courseId);
        }

        public async Task<StudentGradeDetail> GetStudentGradeDetailAsync(string studentId, int courseId)
        {
            return await _teacherCourseRepository.GetStudentGradeDetailAsync(studentId, courseId);
        }

        public async Task<bool> UpdateActivityScoreAsync(string studentId, int activityId, int newScore)
        {
            return await _teacherCourseRepository.UpdateActivityScoreAsync(studentId, activityId, newScore);
        }

    }
}
