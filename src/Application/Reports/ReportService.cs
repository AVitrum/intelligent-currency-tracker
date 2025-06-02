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

    public ReportService(
        ILogger<ReportService> logger,
        IReportRepository reportRepository,
        IAppSettings appSettings)
    {
        _logger = logger;
        _reportRepository = reportRepository;

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
            Description = dto.Description
        };
        const string path = "reports/files/";

        if (dto.Attachments is not null && dto.Attachments.Any())
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

    public async Task<BaseResult> GetByIdAsync(Guid id)
    {
        Report report = await _reportRepository.GetByIdAsync(id);
        ReportDto reportDto = new ReportDto
        {
            Id = report.Id,
            Title = report.Title,
            Description = report.Description,
            IsResolved = report.IsResolved,
            Status = report.Status.ToString()
        };

        if (report.Attachments.Count != 0)
        {
            List<byte[]> fileBytesList = [];

            foreach (FileLink attachment in report.Attachments)
            {
                string key = attachment.Key;

                GetObjectRequest getRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                using GetObjectResponse? response = await _s3Client.GetObjectAsync(getRequest);
                await using Stream? stream = response.ResponseStream;
                using MemoryStream ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                byte[] fileBytes = ms.ToArray();
                fileBytesList.Add(fileBytes);
            }

            reportDto.Attachments = fileBytesList;
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
            Status = report.Status.ToString()
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