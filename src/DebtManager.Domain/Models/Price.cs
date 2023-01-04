namespace DebtManager.Domain.Models
{
    public class Price
    {
        public Guid Id { get; set; }
        public Product? Product { get; set; }
        public Business? Business { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }
}
