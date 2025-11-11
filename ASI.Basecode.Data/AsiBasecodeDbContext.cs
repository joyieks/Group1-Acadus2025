using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;

namespace ASI.Basecode.Data
{
    public partial class AsiBasecodeDBContext : DbContext
    {
        public AsiBasecodeDBContext()
        {
        }

        public AsiBasecodeDBContext(DbContextOptions<AsiBasecodeDBContext> options)
            : base(options)
        {
        }

        //Students
        //public virtual DbSet<TaskItem> TaskItem { get; set; }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<StudentProfile> StudentProfiles { get; set; }
        public virtual DbSet<TeacherProfile> TeacherProfiles { get; set; }
        public virtual DbSet<AdminProfile> AdminProfiles { get; set; }
        public virtual DbSet<Semester> Semesters { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Program> Programs { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<EmergencyContact> EmergencyContacts { get; set; }
        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<CourseEnrollment> CourseEnrollments { get; set; }
        public virtual DbSet<Activity> Activities { get; set; }
        public virtual DbSet<ActivitySubmission> ActivitySubmissions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.id);

                entity.ToTable("Users");

                entity.Property(e => e.id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("id");

                entity.Property(e => e.firstName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.lastName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.middleName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.suffix)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.email)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.contactNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.userTypeId);

                entity.Property(e => e.isActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.profilePictureUrl)
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
