using Amazon.S3;
using Amazon.S3.Model;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Application.Reports.Results;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Shared.Dtos;

namespace Application.Reports;

public class ReportService : IReportService
{
    private readonly string _bucketName;
    private readonly ILogger<ReportService> _logger;
    private readonly IReportRepository _reportRepository;
    private readonly IAmazonS3 _s3Client;
    private readonly IEmailSender _emailSender;
    private readonly IUserService _userService;

    public ReportService(
        ILogger<ReportService> logger,
        IReportRepository reportRepository,
        IAppSettings appSettings,
        IEmailSender emailSender,
        IUserService userService)
    {
        _logger = logger;
        _reportRepository = reportRepository;
        _emailSender = emailSender;
        _userService = userService;

        _s3Client = new AmazonS3Client(
            appSettings.AwsAccessKey,
            appSettings.AwsSecretKey,
            new AmazonS3Config
            {
                ServiceURL = appSettings.AwsEndpoint, ForcePathStyle = true
            });
        _bucketName = appSettings.AwsBucket;
    }

    public async Task<BaseResult> CreateAsync(CreateReportDto dto)
    {
        Report report = new Report
        {
            Title = dto.Title,
            Description = dto.Description,
            SenderId = dto.SenderId
        };
        const string path = "reports/files/";

        if (dto.Attachments is not null && dto.Attachments.Count != 0)
        {
            foreach ((string filePath, string fileExtension) in dto.Attachments)
            {
                string key = $"{path}{Guid.NewGuid()}{fileExtension}";

                try
                {
                    PutObjectRequest putRequest = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = key,
                        FilePath = filePath
                    };

                    await _s3Client.PutObjectAsync(putRequest);
                    _logger.LogInformation("File {FilePath} uploaded successfully to {Key}", filePath, key);

                    FileLink fileLink = new FileLink
                    {
                        Key = key,
                        Extension = fileExtension
                    };
                    report.Attachments.Add(fileLink);
                }
                catch (AmazonS3Exception amazonS3Exception)
                {
                    BaseResult.FailureResult([
                        $"Error uploading file {filePath} to S3: {amazonS3Exception.Message}"
                    ]);
                }
            }
        }

        await _reportRepository.AddAsync(report);
        return BaseResult.SuccessResult();
    }

    public async Task<BaseResult> RespondAsync(Guid id, string message)
    {
        Report report = await _reportRepository.GetByIdAsync(id);
        BaseResult findEmailResult = await _userService.GetEmailAsync(report.SenderId);

        if (!findEmailResult.Success)
        {
            return BaseResult.FailureResult(findEmailResult.Errors);
        }

        string email = ((EmailResult)findEmailResult).Email;

        await _emailSender.SendEmailWithDefaultDesignAsync(email, report.Title, message);
        _logger.LogInformation("Response sent to {Email} for report {ReportId}", email, id);

        report.IsResolved = true;
        report.Status = ReportStatus.Resolved;
        await _reportRepository.UpdateAsync(report);

        return BaseResult.SuccessResult();
    }

    public async Task<BaseResult> GetByIdAsync(Guid id)
    {
        Report report = await _reportRepository.GetByIdAsync(id);
        ReportDto reportDto = new ReportDto
        {
            Id = report.Id,
            Title = report.Title,
            Description = report.Description,
            IsResolved = report.IsResolved,
            Status = report.Status.ToString(),
            SenderId = report.SenderId
        };

        if (report.Attachments.Count > 0)
        {
            List<string> publicUrls = [];
            string serviceUrl = _s3Client.Config.ServiceURL;

            string baseUriString = serviceUrl.EndsWith("/") ? serviceUrl : serviceUrl + "/";
            Uri baseUri = new Uri(baseUriString);

            foreach (FileLink attachment in report.Attachments)
            {
                string key = attachment.Key;
                string fullUrl = new Uri(baseUri, $"{_bucketName.Trim('/')}/{key.TrimStart('/')}").ToString();

                publicUrls.Add(fullUrl);
            }

            reportDto.Attachments = publicUrls;
        }
        else
        {
            reportDto.Attachments = new List<string>();
        }

        return GetReportByIdResult.SuccessResult(reportDto);
    }

    public async Task<BaseResult> GetAllAsync(int page, int pageSize)
    {
        List<Report> reports = (await _reportRepository.GetAllAsync(page, pageSize)).ToList();
        List<ReportDto> reportDtos = reports.Select(report => new ReportDto
        {
            Id = report.Id,
            Title = report.Title,
            Description = report.Description,
            IsResolved = report.IsResolved,
            Status = report.Status.ToString(),
            SenderId = report.SenderId
        }).ToList();

        return GetAllReportsResult.SuccessResult(reportDtos);
    }

    public async Task<BaseResult> MarkAsResolvedAsync(Guid id)
    {
        Report report = await _reportRepository.GetByIdAsync(id);

        report.IsResolved = true;
        report.Status = ReportStatus.Resolved;
        await _reportRepository.UpdateAsync(report);

        return BaseResult.SuccessResult();
    }
}