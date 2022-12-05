namespace DebtManager.Domain.Models
{
    public class DebtDetail
    {
        public Guid Id { get; set; }
        public Debt Debt { get; set; }
        public Product Product { get; set; }
    }
}
