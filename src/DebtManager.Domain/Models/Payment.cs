namespace DebtManager.Domain.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public DebtDetailUser? DebtDetailUser { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public enum PaymentStatus
        {
            None = 0,
            Draft = 1,
            Approved = 2,
            Denied = 3
        }
    }
}
