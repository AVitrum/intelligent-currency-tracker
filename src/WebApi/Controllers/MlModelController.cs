using Application.Common.Payload.Dtos;
using Application.Common.Payload.Requests;
using Application.ExchangeRates.Results;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;

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

    [HttpPost("train-model")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TrainModelAsync(ExchangeRateRequest request)
    {
        BaseResult result = await _mlModelService.TrainModelAsync(request);
        return result.Success ? Ok("Training has begun!") : BadRequest(result.Errors);
    }

    [HttpGet("predict")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PredictAsync([FromQuery] ExchangeRatePredictionDto dto)
    {
        BaseResult result = await _mlModelService.PredictAsync(dto);
        if (result is ExchangeRatePredictionResult predictionResult) return Ok(predictionResult.Prediction);

        return BadRequest(result.Errors);
    }
}