using Application.Common.Interfaces.Services;
using Application.Reports.Results;
using Domain.Common;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Shared.Dtos;
using Shared.Payload.Responses;
using Shared.Payload.Responses.Report;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpPost("send")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResponse))]
    public async Task<ActionResult<BaseResponse>> SendReport(
        [FromForm] string title,
        [FromForm] string description,
        IFormFileCollection? attachments)
    {
        BaseResponse response;

        if (string.IsNullOrWhiteSpace(title))
        {
            response = new DefaultResponse(
                false,
                "Title is required.",
                StatusCodes.Status400BadRequest,
                new List<string> { "Title is required." });
            return BadRequest(response);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            response = new DefaultResponse(
                false,
                "Description is required.",
                StatusCodes.Status400BadRequest,
                new List<string> { "Description is required." });
            return BadRequest(response);
        }

        CreateReportDto dto = new CreateReportDto
        {
            Title = title,
            Description = description
        };

        if (attachments is not null && attachments.Any())
        {
            foreach (IFormFile file in attachments)
                if (file.Length > 0)
                {
                    string filePath = Path.GetTempFileName();
                    await using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    string fileExtension = Path.GetExtension(file.FileName);

                    dto.Attachments ??= new List<(string, string)>();
                    dto.Attachments.Add((filePath, fileExtension));
                }
        }

        BaseResult result = await _reportService.CreateAsync(dto);

        if (result.Success)
        {
            response = new DefaultResponse(
                true,
                "Report sent successfully.",
                StatusCodes.Status200OK,
                []);
            return Ok(response);
        }

        response = new DefaultResponse(
            false,
            "Failed to send report.",
            StatusCodes.Status500InternalServerError,
            result.Errors);
        return StatusCode(StatusCodes.Status500InternalServerError, response);
    }

    [HttpGet("get/{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetReportByIdResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResponse))]
    public async Task<ActionResult<BaseResponse>> GetReport(Guid id)
    {
        BaseResponse response;

        if (id == Guid.Empty)
        {
            response = new DefaultResponse(
                false,
                "Invalid report ID.",
                StatusCodes.Status400BadRequest,
                new List<string> { "Invalid report ID." });
            return BadRequest(response);
        }

        BaseResult result = await _reportService.GetByIdAsync(id);

        if (result is GetReportByIdResult extendedResult)
        {
            response = new GetReportByIdResponse(
                true,
                "Report retrieved successfully.",
                StatusCodes.Status200OK,
                [],
                extendedResult.ReportDto);
            return Ok(response);
        }

        response = new DefaultResponse(
            false,
            "Report not found.",
            StatusCodes.Status404NotFound,
            result.Errors);
        return NotFound(response);
    }

    [HttpGet("get-all")]
    [Authorize(Roles = nameof(UserRole.ADMIN))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAllReportsResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResponse))]
    public async Task<ActionResult<BaseResponse>> GetAllReports([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        BaseResult result = await _reportService.GetAllAsync(page, pageSize);

        if (result is GetAllReportsResult extendedResult)
        {
            GetAllReportsResponse response = new GetAllReportsResponse(
                true,
                "Reports retrieved successfully.",
                StatusCodes.Status200OK,
                [],
                extendedResult.Reports);
            return Ok(response);
        }

        BaseResponse errorResponse = new DefaultResponse(
            false,
            "Failed to retrieve reports.",
            StatusCodes.Status500InternalServerError,
            result.Errors);
        return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
    }

    [HttpPatch("mark-as-resolved/{id:guid}")]
    [Authorize(Roles = nameof(UserRole.ADMIN))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResponse))]
    public async Task<ActionResult<BaseResponse>> MarkReportAsResolved(Guid id)
    {
        BaseResponse response;

        if (id == Guid.Empty)
        {
            response = new DefaultResponse(
                false,
                "Invalid report ID.",
                StatusCodes.Status400BadRequest,
                new List<string> { "Invalid report ID." });
            return BadRequest(response);
        }

        BaseResult result = await _reportService.MarkAsResolvedAsync(id);

        if (result.Success)
        {
            response = new DefaultResponse(
                true,
                "Report marked as resolved successfully.",
                StatusCodes.Status200OK,
                []);
            return Ok(response);
        }

        response = new DefaultResponse(
            false,
            "Failed to mark report as resolved.",
            StatusCodes.Status500InternalServerError,
            result.Errors);
        return StatusCode(StatusCodes.Status500InternalServerError, response);
    }
}