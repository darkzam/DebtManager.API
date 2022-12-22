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
        //Validate each DebtdetailDto
        //Transform Dtos into entities
        //Execute service logic
        //Return Results

        //var results = await unitOfWork.DebtDetailRepository.CreateCollection();


        return null;
    }
}
