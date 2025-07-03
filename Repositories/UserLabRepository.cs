using GradProject.Data;
using GradProject.Models.Entities;
using Microsoft.EntityFrameworkCore; // Added for Include extension method
using System.Linq.Expressions; // Added for Expression

namespace GradProject.Repositories
{
    public class UserLabRepository : Repository<UserLab>, IUserLabRepository
    {
        public UserLabRepository(AppDbContext context) : base(context)
        {
        }

        // Override Find to include Lab entity
        public override async Task<IEnumerable<UserLab>> Find(Expression<Func<UserLab, bool>> predicate)
        {
            return await _context.UserLabs.Include(ul => ul.Lab).Where(predicate).ToListAsync();
        }
    }
}