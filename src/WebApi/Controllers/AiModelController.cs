using Application.AiModel.Results;
using Application.Common.Interfaces.Services;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Shared.Payload.Requests;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class AiModelController : ControllerBase
{
    private readonly IAiModelService _aiModelService;

    public AiModelController(IAiModelService aiModelService)
    {
        _aiModelService = aiModelService;
    }

    [HttpPost("train")]
    public async Task<IActionResult> Train(TrainModelRequest request)
    {
        BaseResult result = await _aiModelService.TrainModelAsync(request.CurrencyR030);

        if (result is TrainResult trainResult)
        {
            return Ok(trainResult.Message);
        }

        return BadRequest(result);
    }

    [HttpPost("predict")]
    public async Task<IActionResult> Predict(PredictRequest request)
    {
        BaseResult result = await _aiModelService.PredictAsync(request.CurrencyR030, request.Date);

        if (result is PredictResult predictResult)
        {
            return Ok(predictResult.Prediction);
        }

        return BadRequest(result);
    }
}