namespace GradProject.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ILabRepository Labs { get; }
        IUserLabRepository UserLabs { get; }
        Task<int> Complete();
    }
}