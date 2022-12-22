using DebtManager.Domain.Models;

public class DebtDetailDto
{
    public Guid Id { get; set; }
    public string DebtCode { get; set; }
    public string ProductName { get; set; }
    public string CreatedDate { get; set; }
    public string UpdatedDate { get; set; }

    public DebtDetailDto()
    { }

    public DebtDetailDto(DebtDetail debtDetail)
    {
        Id = debtDetail.Id;
        DebtCode = debtDetail.Debt?.Code;
        ProductName = debtDetail.Product?.Name;
        CreatedDate = debtDetail.CreatedDate.ToString("dd/mm/yyyy");
        UpdatedDate = debtDetail.UpdatedDate.ToString("dd/mm/yyyy");
    }
}
