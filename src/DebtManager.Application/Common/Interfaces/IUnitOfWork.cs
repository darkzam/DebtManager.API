namespace DebtManager.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        IDebtRepository DebtRepository { get; }
        Task<int> CompleteAsync();
    }
}
