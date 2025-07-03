namespace GradProject.Models.Entities
{
    public class Lab
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ImageName { get; set; }
        public int VulnerabilityId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Solution { get; set; }
        public string? Flag { get; set; }
        public DifficultyLevel? Difficulty { get; set; }
        public DateTime? CreatedAt { get; set; }

        public Vulnerability? Vulnerability { get; set; }
        public ICollection<UserLab> UserLabs { get; set; } = new List<UserLab>();
    }

    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }
}
