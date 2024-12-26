using Domain.Common;

namespace Application.ExchangeRates.Results;

public class ExchangeRatePredictionResult : BaseResult
{
    public readonly string Prediction;
    
    private ExchangeRatePredictionResult(bool success, IEnumerable<string> errors, string prediction) : base(success, errors)
    {
        Prediction = prediction;
    }

    public static ExchangeRatePredictionResult SuccessResult(string prediction) => new(true, [], prediction);
}