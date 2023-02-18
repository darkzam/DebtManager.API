using DebtManager.Domain.Models;

namespace DebtManager.API.Models
{
    public class DebtDetailChargesEntry
    {
        public DebtDetail DebtDetail { get; set; }
        public decimal Total { get; set; }
        public List<User> Users { get; set; }
    }
}
