using DebtManager.API.Models;

public class ProductCharges
{
    public string ProductName { get; set; }
    public IEnumerable<ChargePorcentage> Charges { get; set; }
}
