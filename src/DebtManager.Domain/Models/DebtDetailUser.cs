namespace DebtManager.Domain.Models
{
    public class DebtDetailUser
    {
        public Guid Id { get; set; }
        public DebtDetail DebtDetail { get; set; }
        public User User { get; set; }
    }
}
