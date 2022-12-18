public class DebtDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public Guid HostId { get; set; }
    public string Title { get; set; }
    public decimal Total { get; set; }
    public decimal ServiceRate { get; set; }
    public string CreatedDate { get; set; }
    public string UpdatedDate { get; set; }
}
