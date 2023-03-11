using DebtManager.Domain.Models;

public class DebtDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Username { get; set; }
    public string BusinessName { get; set; }
    public string Title { get; set; }
    public decimal Total { get; set; }
    public decimal ServiceRate { get; set; }
    public bool IpoconsumoTax { get; set; }
    public string CreatedDate { get; set; }
    public string UpdatedDate { get; set; }

    public DebtDto()
    { }

    public DebtDto(Debt debt)
    {
        Id = debt.Id;
        Code = debt.Code;
        Username = debt.Host?.Username;
        BusinessName = debt.Business?.Name;
        Title = debt.Title;
        Total = debt.Total;
        ServiceRate = debt.ServiceRate;
        IpoconsumoTax = debt.IpoconsumoTax;
        CreatedDate = debt.CreatedDate.ToString("dd/mm/yyyy");
        UpdatedDate = debt.UpdatedDate.ToString("dd/mm/yyyy");
    }
}
