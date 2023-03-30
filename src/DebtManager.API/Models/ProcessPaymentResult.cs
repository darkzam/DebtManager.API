using DebtManager.Domain.Models;

namespace DebtManager.API.Models
{
    public class ProcessPaymentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Payment Payment { get; set; }
        public IEnumerable<Payment> Payments { get; set; }
        public EntityOperation Operation { get; set; }
    }
}
