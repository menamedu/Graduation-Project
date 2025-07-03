using Microsoft.AspNetCore.Identity;


namespace GradProject.Models.Entities
{
    public class User : IdentityUser<int>
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<UserLab> UserLabs { get; set; } = new List<UserLab>();
    }
}