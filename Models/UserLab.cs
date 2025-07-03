//using GradProject.DTOs;

namespace GradProject.Models.Entities
{
    public enum LabStatus
    {
        NotCompleted,
        Completed
    }


    public class UserLab
    {
        public int UserId { get; set; }
        public int LabId { get; set; }
        public LabStatus Status { get; set; } = LabStatus.NotCompleted;
        public int Score { get; set; } = 0;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public Lab? Lab { get; set; }
    }
}