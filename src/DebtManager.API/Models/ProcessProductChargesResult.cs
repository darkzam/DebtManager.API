using DebtManager.Domain.Models;

namespace DebtManager.API.Models
{
    //TO-DO: Create an abstract Generic Result.
    public class ProcessProductChargesResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public IEnumerable<DebtDetailUser> Charges { get; set; }
        public EntityOperation Operation { get; set; }
    }
}
