namespace DebtManager.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
