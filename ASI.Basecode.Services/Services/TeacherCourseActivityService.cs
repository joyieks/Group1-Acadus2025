using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Service.ServiceModels;


namespace ASI.Basecode.Services.Services
{
    public class TeacherCourseActivityService : ITeacherCourseActivityService
    {
        private readonly ITeacherCourseActivityRepository _repo;

        public TeacherCourseActivityService(ITeacherCourseActivityRepository repo)
        {
            _repo = repo;
        }


        // ========================= LOAD PAGE =========================
        public async Task<TeacherCourseModel> LoadCourseActivityPageAsync(int courseId)
        {
            var activities = await _repo.GetActivitiesByCourseAsync(courseId);
            var students = await _repo.GetStudentsByCourseIdAsync(courseId);
            var submissions = await _repo.GetSubmissionsByCourseAsync(courseId);

            return new TeacherCourseModel
            {
                CourseId = courseId,

                Activities = activities.Select(a => new TeacherActivityModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    CourseId = a.CourseId,
                    MaxScore = a.maxScore,
                    DueDate = a.DueDate,
                    CreatedAt = a.CreatedAt,
                    IsVisible = a.IsVisible,
                    Invisible_At = a.Invisible_At
                }).ToList(),

                Students = students.Select(s => new TeacherStudentModel
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    MiddleName = s.MiddleName,
                    LastName = s.LastName,
                    IsActive = s.IsActive
                }).ToList(),

                Submissions = submissions.Select(sub => new TeacherActivitySubmissionModel
                {
                    Id = sub.Id,
                    ActivityId = sub.ActivityId,
                    StudentId = sub.StudentId,
                    Score = sub.Score,
                    SubmissionStatus = sub.SubmissionStatus,
                    CreatedAt = sub.CreatedAt
                }).ToList()
            };
        }


        // ========================= CREATE ACTIVITY =========================
        public async Task CreateActivityAsync(TeacherActivityModel model)
        {
            var activity = new ActivityModel
            {
                Title = model.Title,
                Description = model.Description,
                CourseId = model.CourseId,
                maxScore = model.MaxScore,
                CreatedAt = DateTime.UtcNow,
                IsVisible = model.IsVisible
            };

            await _repo.CreateActivityAsync(activity);
        }


        // ========================= UPDATE ACTIVITY =========================
        public async Task UpdateActivityAsync(TeacherActivityModel vm)
        {
            var activity = await _repo.GetActivityByIdAsync(vm.Id);

            activity.Title = vm.Title;
            activity.Description = vm.Description;
            activity.maxScore = vm.MaxScore;
            activity.DueDate = vm.DueDate;
            activity.IsVisible = vm.IsVisible;
            activity.Invisible_At = vm.Invisible_At;

            await _repo.UpdateActivityAsync(activity);
        }

        // ========================= GET ACTIVITY DETAILS =========================
        public async Task<TeacherActivityModel> GetActivityDetailsAsync(int activityId)
        {
            var activity = await _repo.GetActivityByIdAsync(activityId);

            return new TeacherActivityModel
            {
                Id = activity.Id,
                Title = activity.Title,
                Description = activity.Description,
                CourseId = activity.CourseId,
                MaxScore = activity.maxScore,
                DueDate = activity.DueDate,
                CreatedAt = activity.CreatedAt,
                IsVisible = activity.IsVisible,
                Invisible_At = activity.Invisible_At
            };
        }

        public async Task GradeActivityAsync(TeacherActivitySubmissionModel vm)
        {
            var sub = new ActivitySubmissionModel
            {
                ActivityId = vm.ActivityId,
                StudentId = vm.StudentId,
                Score = vm.Score,
                SubmissionStatus = "Graded"
            };

            await _repo.SaveSubmissionAsync(sub);
        }
    }
}
