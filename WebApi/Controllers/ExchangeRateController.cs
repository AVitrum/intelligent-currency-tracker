using Application.ExchangeRates.Results;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExchangeRateController : ControllerBase
{
    private readonly IExchangeRateService _exchangeRateService;

    public ExchangeRateController(IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }
    
    [HttpGet("fetch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FetchExchangeRatesAsync([FromQuery] ExchangeRatesRangeDto dto)
    {
        var result = await _exchangeRateService.FetchExchangeRatesAsync(dto);
        return result.Success ? Ok() : BadRequest(result.Errors);
    }
    
    [HttpPost("uploadCsv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadCsvAsync([FromForm] CsvFileUploadDto dto)
    {
        var result = await _exchangeRateService.GetExchangeRatesFromCsvAsync(dto.File);
        return result.Success ? Ok() : BadRequest(result.Errors);
    }
    
    [HttpGet("exportCsv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportCsvAsync([FromQuery] ExchangeRatesRangeDto dto)
    {
        var result = await _exchangeRateService.ExportExchangeRatesToCsvAsync(dto);
        
        if (result is ExportExchangeRatesToCsvResult exportResult)
            return File(exportResult.FileContent, "text/csv", exportResult.FileName);
        
        return BadRequest(result.Errors);
    }
    
    [HttpPost("trainModel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TrainModelAsync(ExchangeRatesRangeDto dto)
    {
        var result = await _exchangeRateService.TrainModelAsync(dto);
        return result.Success ? Ok() : BadRequest(result.Errors);
    }
    
    [HttpGet("getRange")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRangeAsync([FromQuery] ExchangeRatesRangeDto dto)
    {
        var result = await _exchangeRateService.GetRangeAsync(dto);

        if (result is not GetExchangeRateRangeResult rangeResult) return BadRequest(result.Errors);
        
        return Ok(rangeResult.Data);
    }
}