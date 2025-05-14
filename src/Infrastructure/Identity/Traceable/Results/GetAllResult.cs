using Domain.Common;
using Shared.Dtos;

namespace Infrastructure.Identity.Traceable.Results;

public class GetAllResult : BaseResult
{
    private GetAllResult(bool success, IEnumerable<string> errors, IEnumerable<CurrencyDto> data) : base(success,
        errors)
    {
        Data = data;
    }

    public IEnumerable<CurrencyDto> Data { get; }

    public static GetAllResult SuccessResult(IEnumerable<CurrencyDto> currencies)
    {
        return new GetAllResult(true, [], currencies);
    }
}