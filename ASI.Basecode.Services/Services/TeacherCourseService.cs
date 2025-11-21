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
        private readonly ITeacherCourseRepository _repo;

        public TeacherCourseService(ITeacherCourseRepository repo)
        {
            _repo = repo;
        }

        public async Task<CourseGradebookViewModel> GetCourseGradebookAsync(int courseId)
        {
            var course = await _repo.GetCourseAsync(courseId);
            if (course == null) return null;

            var enrollments = await _repo.GetActiveEnrollmentsAsync(courseId);
            var activities = await _repo.GetActivitiesByCourseIdAsync(courseId);

            var viewModel = new CourseGradebookViewModel
            {
                CourseId = course.Id,
                CourseCode = course.Code,
                CourseTitle = course.Name,
                SemesterInfo = course.SemesterId,
                CardColor = "#E8F9E8"
            };

            foreach (var enrollment in enrollments)
            {
                var submissions = await _repo.GetSubmissionsByStudentAndCourseAsync(enrollment.StudentId, courseId);
                var graded = submissions
                    .Where(s => s.SubmissionStatus == "Graded")
                    .Select(s =>
                    {
                        var activity = activities.FirstOrDefault(a => a.Id == s.ActivityId);
                        return (activity == null || activity.maxScore <= 0) ? 0.0 :
                               (double)s.Score / activity.maxScore * 100;
                    }).ToList();

                var average = graded.Any() ? Math.Round(graded.Average(), 1) : 0;

                var studentUser = await _repo.GetUserByUserTypeIdAsync(enrollment.StudentId);

                viewModel.Students.Add(new StudentGradeItem
                {
                    StudentId = enrollment.StudentId,
                    StudentDisplayId = studentUser?.UserDisplayId ?? "N/A",
                    Name = $"{studentUser?.FirstName} {studentUser?.LastName}".Trim(),
                    AveragePercentage = average
                });
            }

            viewModel.Activities = activities.Select(a => new ActivityGradeItem
            {
                ActivityName = a.Title,
                MaxScore = a.maxScore,
                RawScore = 0
            }).ToList();

            return viewModel;
        }

        public async Task<StudentGradeDetail> GetStudentGradeDetailAsync(string studentId, int courseId)
        {
            var activities = await _repo.GetActivitiesByCourseIdAsync(courseId);
            var submissions = await _repo.GetSubmissionsByStudentAndCourseAsync(studentId, courseId);
            var user = await _repo.GetUserByUserTypeIdAsync(studentId);

            var detail = new StudentGradeDetail
            {
                StudentId = studentId,
                StudentDisplayId = user?.UserDisplayId ?? "N/A",
                Name = $"{user?.FirstName} {user?.LastName}".Trim()
            };

            foreach (var activity in activities)
            {
                var submission = submissions.FirstOrDefault(s =>
                    s.ActivityId == activity.Id && s.SubmissionStatus == "Graded");

                if (submission == null) continue;

                detail.TotalRawScore += submission.Score;
                detail.TotalMaxScore += activity.maxScore;

                detail.Activities.Add(new ActivityGradeItem
                {
                    ActivityName = activity.Title,
                    ActivityId = activity.Id,
                    RawScore = submission.Score,
                    MaxScore = activity.maxScore
                });
            }

            return detail;
        }

        public async Task<bool> UpdateActivityScoreAsync(string studentId, int activityId, int newScore)
        {
            var submission = await _repo.GetSubmissionAsync(studentId, activityId);
            if (submission == null) return false;

            submission.Score = newScore;
            return await _repo.UpdateSubmissionAsync(submission);
        }
    }

}
