using GradProject.Data;
using GradProject.Models.Entities;

namespace GradProject.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        // Implement any user-specific methods here if needed in the future
    }
}