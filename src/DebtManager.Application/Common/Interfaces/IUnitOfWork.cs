namespace DebtManager.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        IDebtRepository DebtRepository { get; }
        IUserRepository UserRepository { get; }
        Task<int> CompleteAsync();
    }
}
