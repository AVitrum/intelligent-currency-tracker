using Application.AiModel.Results;
using Application.Common.Interfaces.Services;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Shared.Payload.Requests;
using Shared.Payload.Responses.AiModel;

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
    public async Task<ActionResult<BaseResponse>> Train(TrainModelRequest request)
    {
        BaseResponse response;
        BaseResult result = await _aiModelService.TrainModelAsync(request.CurrencyR030);

        if (result is TrainResult trainResult)
        {
            response = new TrainResponse(
                trainResult.Success,
                trainResult.Message,
                StatusCodes.Status200OK,
                trainResult.Errors);

            return Ok(response);
        }

        response = new TrainResponse(
            result.Success,
            "Model not trained.",
            StatusCodes.Status400BadRequest,
            result.Errors);

        return BadRequest(response);
    }

    [HttpPost("predict")]
    public async Task<ActionResult<BaseResponse>> Predict(PredictRequest request)
    {
        BaseResponse response;
        BaseResult result = await _aiModelService.PredictAsync(request.CurrencyR030, request.Date);

        if (result is PredictResult predictResult)
        {
            response = new PredictResponse(
                predictResult.Success,
                "Prediction successful.",
                StatusCodes.Status200OK,
                predictResult.Errors,
                predictResult.Prediction);
            return Ok(response);
        }

        response = new PredictResponse(
            result.Success,
            "Prediction failed.",
            StatusCodes.Status400BadRequest,
            result.Errors,
            null);

        return BadRequest(response);
    }

    [HttpPost("forecast")]
    public async Task<ActionResult<BaseResponse>> Forecast(ForecastRequest request)
    {
        BaseResponse response;
        BaseResult result = await _aiModelService.ForecastAsync(request.CurrencyR030, request.Periods);

        if (result is ForecastResult forecastResult)
        {
            response = new ForecastResponse(
                forecastResult.Success,
                "Forecast successful.",
                StatusCodes.Status200OK,
                forecastResult.Errors,
                forecastResult.Forecast);
            return Ok(response);
        }

        response = new ForecastResponse(
            result.Success,
            "Forecast failed.",
            StatusCodes.Status400BadRequest,
            result.Errors,
            null);

        return BadRequest(response);
    }
}