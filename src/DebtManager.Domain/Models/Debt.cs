namespace DebtManager.Domain.Models
{
    public class Debt
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public User? Host { get; set; }
        public string Title { get; set; }
        public decimal Total { get; set; }
        public decimal ServiceRate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
