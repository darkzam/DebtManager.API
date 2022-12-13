namespace DebtManager.Domain.Models
{
    public class DebtDetailUser
    {
        public Guid Id { get; set; }
        public DebtDetail? DebtDetail { get; set; }
        public User? User { get; set; }
        public decimal Porcentage { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
