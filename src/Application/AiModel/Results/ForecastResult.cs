using Domain.Common;

namespace Application.AiModel.Results;

public class ForecastResult : BaseResult
{
    public List<Prediction> Forecast { get; }

    private ForecastResult(bool success, IEnumerable<string> errors, List<Prediction> forecast) : base(success,
        errors)
    {
        Forecast = forecast;
    }

    public static ForecastResult SuccessResult(List<Prediction> forecast)
    {
        return new ForecastResult(true, [], forecast);
    }
}