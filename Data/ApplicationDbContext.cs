using HRMS.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<PerformanceReview> PerformanceReviews { get; set; }
        public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<Timesheet> Timesheets { get; set; }
        public DbSet<TimesheetEntry> TimesheetEntries { get; set; }
        public DbSet<TimesheetAuditLog> TimesheetAuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Employee Configuration
            builder.Entity<Employee>(entity =>
            {
                entity.HasIndex(e => e.EmployeeId).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasOne(e => e.Department)
                      .WithMany(d => d.Employees)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Manager)
                      .WithMany()
                      .HasForeignKey(e => e.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Attendance Configuration
            builder.Entity<Attendance>(entity =>
            {
                entity.HasIndex(a => new { a.EmployeeId, a.Date }).IsUnique();
                entity.HasOne(a => a.Employee)
                      .WithMany()
                      .HasForeignKey(a => a.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Leave Configuration
            builder.Entity<Leave>(entity =>
            {
                entity.HasOne(l => l.Employee)
                      .WithMany()
                      .HasForeignKey(l => l.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(l => l.ApprovedBy)
                      .WithMany()
                      .HasForeignKey(l => l.ApprovedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Payroll Configuration
            builder.Entity<Payroll>(entity =>
            {
                entity.HasOne(p => p.Employee)
                      .WithMany()
                      .HasForeignKey(p => p.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Performance Review Configuration
            builder.Entity<PerformanceReview>(entity =>
            {
                entity.HasOne(p => p.Employee)
                      .WithMany()
                      .HasForeignKey(p => p.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(p => p.ReviewedBy)
                      .WithMany()
                      .HasForeignKey(p => p.ReviewedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Employee Document Configuration
            builder.Entity<EmployeeDocument>(entity =>
            {
                entity.HasOne(d => d.Employee)
                      .WithMany(e => e.Documents)
                      .HasForeignKey(d => d.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Company Configuration
            builder.Entity<Company>(entity =>
            {
                entity.HasIndex(c => c.Email).IsUnique();
            });

            // Employee -> Company relationship
            builder.Entity<Employee>()
                   .HasOne(e => e.Company)
                   .WithMany(c => c.Employees)
                   .HasForeignKey(e => e.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Employee OnboardingToken index for fast lookup
            builder.Entity<Employee>()
                   .HasIndex(e => e.OnboardingToken)
                   .IsUnique()
                   .HasFilter("[OnboardingToken] IS NOT NULL");

            // Job Posting Configuration
            builder.Entity<JobPosting>(entity =>
            {
                entity.HasIndex(j => j.Status);
                entity.HasIndex(j => j.JobPostingId).IsUnique();
            });

            // Job Application Configuration
            builder.Entity<JobApplication>(entity =>
            {
                entity.HasOne(a => a.JobPosting)
                      .WithMany(j => j.Applications)
                      .HasForeignKey(a => a.JobPostingId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(a => new { a.JobPostingId, a.NormalizedCandidateEmail }).IsUnique();
            });

            // Timesheet Configuration
            builder.Entity<Timesheet>(entity =>
            {
                entity.HasIndex(t => new { t.EmployeeId, t.Month, t.Year }).IsUnique();
                entity.HasOne(t => t.Employee)
                      .WithMany()
                      .HasForeignKey(t => t.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.ApprovedBy)
                      .WithMany()
                      .HasForeignKey(t => t.ApprovedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // TimesheetEntry Configuration
            builder.Entity<TimesheetEntry>(entity =>
            {
                entity.HasOne(e => e.Timesheet)
                      .WithMany(t => t.Entries)
                      .HasForeignKey(e => e.TimesheetId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // TimesheetAuditLog Configuration
            builder.Entity<TimesheetAuditLog>(entity =>
            {
                entity.HasOne(a => a.Timesheet)
                      .WithMany(t => t.AuditLogs)
                      .HasForeignKey(a => a.TimesheetId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
