namespace DebtManager.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        IDebtRepository DebtRepository { get; }
        IUserRepository UserRepository { get; }
        IDebtDetailRepository DebtDetailRepository { get; }
        IProductRepository ProductRepository { get; }
        Task<int> CompleteAsync();
    }
}
