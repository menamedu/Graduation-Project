using GradProject.Data;
using GradProject.Models.Entities;

namespace GradProject.Repositories
{
    public class LabRepository : Repository<Lab>, ILabRepository
    {
        public LabRepository(AppDbContext context) : base(context)
        {
        }

        // Implement any lab-specific methods here if needed in the future
    }
}