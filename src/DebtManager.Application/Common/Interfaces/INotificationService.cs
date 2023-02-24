namespace DebtManager.Application.Common.Interfaces
{
    public interface INotificationService
    {
        Task Notify(string message);
    }
}
