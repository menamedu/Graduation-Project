using GradProject.Models.Entities; // Replace with your namespace
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GradProject.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DbSet<Vulnerability> Vulnerabilities { get; set; }
        public DbSet<Lab> Labs { get; set; }
        public DbSet<UserLab> UserLabs { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Required for Identity

            // Configure composite key for UserVulnerability
            modelBuilder.Entity<UserLab>()
                .HasKey(uv => new { uv.UserId, uv.LabId });
            modelBuilder.Entity<UserLab>()
                .HasOne(uv => uv.User)
                .WithMany(u => u.UserLabs)
                .HasForeignKey(uv => uv.UserId);
            modelBuilder.Entity<UserLab>()
                .HasOne(uv => uv.Lab)
                .WithMany(l => l.UserLabs)
                .HasForeignKey(uv => uv.LabId);

            // Configure relationships


            modelBuilder.Entity<Lab>()
                .HasOne(l => l.Vulnerability)
                .WithMany(v => v.Labs)
                .HasForeignKey(l => l.VulnerabilityId);

            modelBuilder.Entity<Vulnerability>().HasData
            (
                new Vulnerability { Id = 1, Name = "SQL Injection", Description = "Allows attackers to execute arbitrary SQL code.", Severity = "High" },
                new Vulnerability { Id = 2, Name = "Cross-Site Scripting (XSS)", Description = "Allows attackers to inject malicious scripts into web pages viewed by other users.", Severity = "Medium" },
                new Vulnerability { Id = 3, Name = "Cross-Site Request Forgery (CSRF)", Description = "Forces an end user to execute unwanted actions on a web application in which they are currently authenticated.", Severity = "Medium" },
                new Vulnerability { Id = 4, Name = "Path Traversal", Description = "Allows attackers to access files and directories that are stored outside the web root folder by manipulating variables that reference files with 'dot-dot-slash (../)' sequences.", Severity = "High" }
            );
            modelBuilder.Entity<Lab>().HasData
            (
                new Lab { Id = 1, Name = "Low Path Traversal", Description = "Path Traversal Lab", ImageName = "low_path_traversal_image", VulnerabilityId = 4, Flag = "LowPassFlag" }
            );
        }
    }
}
