using DebtManager.Domain.Models;

namespace DebtManager.API.Models
{
    public class ProcessDetailGroupResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public IEnumerable<DebtDetail> DebtDetails { get; set; }
    }
}
