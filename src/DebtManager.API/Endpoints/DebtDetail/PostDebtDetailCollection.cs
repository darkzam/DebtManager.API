using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;

public class PostDebtDetailCollection : BaseEndpoint<DebtDetail>
{
    public PostDebtDetailCollection(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapPost($"{Route.OriginalString}/collection", ProcessRequest);
    }

    private async Task<IResult> ProcessRequest(IEnumerable<DebtDetailDto> debtDetailDtos,
                                               IUnitOfWork unitOfWork)
    {
        if (debtDetailDtos is null || !debtDetailDtos.Any())
        {
            return Results.BadRequest();
        }

        var taskResults = debtDetailDtos.Select(x => ProcessDetail(x, unitOfWork));

        var results = await Task.WhenAll(taskResults);

        var debtDetails = results.Where(x => x.DebtDetail != null).Select(x => x.DebtDetail);

        unitOfWork.DebtDetailRepository.CreateCollection(debtDetails);

        await unitOfWork.CompleteAsync();

        //To-Do map DebtDetail entities to Dtos before returning
        return Results.Ok(results);
    }

    private async Task<ProcessDetailResult> ProcessDetail(DebtDetailDto debtDetailDto,
                                                          IUnitOfWork unitOfWork)
    {
        if (string.IsNullOrWhiteSpace(debtDetailDto.DebtCode))
        {
            return new ProcessDetailResult
            {
                Success = false,
                Message = $"{nameof(debtDetailDto.DebtCode)} is invalid."
            };
        }

        if (string.IsNullOrWhiteSpace(debtDetailDto.ProductName))
        {
            return new ProcessDetailResult
            {
                Success = false,
                Message = $"{nameof(debtDetailDto.ProductName)} is invalid."
            };
        }

        var debts = await unitOfWork.DebtRepository.SearchBy(x => x.Code == debtDetailDto.DebtCode
                                                                                .Trim()
                                                                                .ToLower()
                                                                                .RemoveAccents());

        if (debts.FirstOrDefault() is null)
        {
            return new ProcessDetailResult
            {
                Success = false,
                Message = $"{nameof(debtDetailDto.DebtCode)} provided does not exist in the system."
            };
        }

        var products = await unitOfWork.ProductRepository.SearchBy(x => x.Name == debtDetailDto.ProductName
                                                                                                .Trim()
                                                                                                .ToLower()
                                                                                                .RemoveAccents());

        var product = products.FirstOrDefault() != null ? products.FirstOrDefault() :
                                                            new Product()
                                                            {
                                                                Name = debtDetailDto.ProductName
                                                                                    .Trim()
                                                                                    .ToLower()
                                                                                    .RemoveAccents(),
                                                                CreatedDate = DateTime.UtcNow,
                                                                UpdatedDate = DateTime.UtcNow
                                                            };

        var debtDetail = new DebtDetail()
        {
            Debt = debts.FirstOrDefault(),
            Product = product,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        return new ProcessDetailResult()
        {
            Success = true,
            DebtDetail = debtDetail,
        };
    }

    public class ProcessDetailResult
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public DebtDetail DebtDetail { get; set; }
    }
}
