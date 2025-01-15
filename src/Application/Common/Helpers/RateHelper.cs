using System.Globalization;
using System.Text;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Payload.Dtos;
using Application.Common.Payload.Requests;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Common.Helpers;

public class RateHelper : IRateHelper
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly ILogger<RateHelper> _logger;
    private readonly IMapper _mapper;
    private readonly IRateRepository _rateRepository;

    public RateHelper(
        ICurrencyRepository currencyRepository,
        IRateRepository rateRepository,
        IMapper mapper,
        ILogger<RateHelper> logger)
    {
        _currencyRepository = currencyRepository;
        _rateRepository = rateRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<Rate>> GetRatesFromRequestAsync(ExchangeRateRequest request)
    {
        IEnumerable<Rate> rates;
        if (request.Currency is null)
        {
            rates = await _rateRepository.GetAsync(request.Start, request.End);
        }
        else
        {
            var currency = await _currencyRepository.GetByCodeAsync(request.Currency)
                           ?? throw new EntityNotFoundException<Currency>();

            rates = await _rateRepository.GetAsync(request.Start, request.End, currency);
        }

        return rates;
    }

    public IEnumerable<RateDto> ConvertRatesToDtoAsync(IEnumerable<Rate> rates)
    {
        var ratesDto = new List<RateDto>();
        ratesDto.AddRange(rates.Select(rate => _mapper.Map<RateDto>(rate)));

        _logger.LogInformation("Successfully converted rates to DTO");
        return ratesDto;
    }

    public async Task<(string, byte[])> ExportExchangeRateToCsvAsync(ExchangeRateRequest request)
    {
        try
        {
            ICollection<Rate> rates;
            var ratesDto = new List<RateDto>();

            var startUtc = request.Start.ToUniversalTime();
            var endUtc = request.End.ToUniversalTime();

            if (request.Currency is null)
            {
                rates = (ICollection<Rate>)await _rateRepository.GetAsync(startUtc, endUtc);
            }
            else
            {
                var currency = await _currencyRepository.GetByCodeAsync(request.Currency)
                               ?? throw new EntityNotFoundException<Currency>();

                rates = (ICollection<Rate>)await _rateRepository.GetAsync(startUtc, endUtc, currency);
            }

            if (rates.Count == 0)
                throw new DataNotFoundException("No exchange rates found for the specified date range");


            ratesDto.AddRange(rates.Select(rate => _mapper.Map<RateDto>(rate)));

            var fileName = $"ExchangeRates_{request.Currency}_{request.Start:yyyyMMdd}_{request.End:yyyyMMdd}.csv";
            var fileContent = await CreateCsvFileAsync(ratesDto);

            _logger.LogInformation("Successfully exported exchange rates to CSV file");
            return (fileName, fileContent);
        }
        catch (DataNotFoundException ex)
        {
            _logger.LogWarning(ex, "No exchange rates found for the specified date range");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while exporting exchange rates to CSV file");
            throw new ExportCsvException("An error occurred while exporting exchange rates to CSV file");
        }
    }

    private async Task<byte[]> CreateCsvFileAsync<T>(IEnumerable<T> content)
    {
        using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        await using var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ","
        });

        await csvWriter.WriteRecordsAsync(content);
        await streamWriter.FlushAsync();

        _logger.LogInformation("Successfully created CSV file");
        return memoryStream.ToArray();
    }
}