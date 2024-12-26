using Application.ExchangeRates.Results;
using Domain.Common;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MlModelController : ControllerBase
{
    private readonly IMlModelService _mlModelService;

    public MlModelController(IMlModelService mlModelService)
    {
        _mlModelService = mlModelService;
    }

    [HttpPost("trainModel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TrainModelAsync(ExchangeRatesRangeDto dto)
    {
        BaseResult result = await _mlModelService.TrainModelAsync(dto);
        return result.Success ? Ok() : BadRequest(result.Errors);
    }
    
    [HttpGet("predict")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PredictAsync([FromQuery] ExchangeRatePredictionDto dto)
    {
        BaseResult result = await _mlModelService.PredictAsync(dto);

        if (result is ExchangeRatePredictionResult predictionResult) return Ok(predictionResult.Prediction);

        return BadRequest(result.Errors);
    }
}