using DebtManager.API.Models;

public class DebtDetailGroupDto
{
    public string ProductName { get; set; }
    public int Amount { get; set; }
    public decimal Total { get; set; }

    public ModelValidationResult ValidateModel()
    {
        if (Amount == 0)
        {
            return new ModelValidationResult
            {
                Success = false,
                Message = $"'{ProductName}' : {nameof(Amount)} is invalid."
            };
        }

        if (Total < 1)
        {
            return new ModelValidationResult
            {
                Success = false,
                Message = $"'{ProductName}' : {nameof(Total)} is invalid."
            };
        }

        if (string.IsNullOrWhiteSpace(ProductName))
        {
            return new ModelValidationResult
            {
                Success = false,
                Message = $"'{ProductName}' : {nameof(ProductName)} is invalid."
            };
        }

        return new ModelValidationResult
        {
            Success = true,
            Message = string.Empty
        };
    }
}
