using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Utils;
using Application.Rates;
using Application.Rates.Results;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Dtos;

namespace Application.Tests;

public class RateServiceTests
{
    private readonly Mock<IRateRepository> _mockRateRepository;
    private readonly Mock<ICurrencyRepository> _mockCurrencyRepository;
    private readonly Mock<IRateHelper> _mockRateHelper;
    private readonly Mock<ILogger<RateService>> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;
    private readonly RateService _rateService;

    public RateServiceTests()
    {
        _mockRateRepository = new Mock<IRateRepository>();
        _mockCurrencyRepository = new Mock<ICurrencyRepository>();
        _mockRateHelper = new Mock<IRateHelper>();
        _mockLogger = new Mock<ILogger<RateService>>();
        _mockMapper = new Mock<IMapper>();

        _rateService = new RateService(
            _mockRateRepository.Object,
            _mockCurrencyRepository.Object,
            _mockRateHelper.Object,
            _mockLogger.Object,
            _mockMapper.Object
        );
    }

    [Fact]
    public async Task GetAllCurrenciesAsync_ShouldReturnCurrencies_WhenCurrenciesExist()
    {
        // Arrange: Setup test data
        var currencies = new List<Currency>
        {
            new Currency { Id = Guid.NewGuid(), R030 = 840, Code = "USD", Name = "US Dollar" },
            new Currency { Id = Guid.NewGuid(), R030 = 978, Code = "EUR", Name = "Euro" }
        };

        var currencyDtos = new List<CurrencyDto>
        {
            new CurrencyDto { R030 = 840, Code = "USD", Name = "US Dollar" },
            new CurrencyDto { R030 = 978, Code = "EUR", Name = "Euro" }
        };

        _mockCurrencyRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(currencies);

        _mockMapper
            .Setup(x => x.Map<CurrencyDto>(It.IsAny<Currency>()))
            .Returns<Currency>(c => new CurrencyDto 
            { 
                R030 = c.R030, 
                Code = c.Code, 
                Name = c.Name 
            });

        // Act: Call the service method
        var result = await _rateService.GetAllCurrenciesAsync();

        // Assert: Verify the result
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Should().BeOfType<GetAllCurrenciesResult>();
        var getAllResult = (GetAllCurrenciesResult)result;
        var dataList = getAllResult.Currencies.ToList();
        dataList.Should().HaveCount(2);
        dataList.Should().Contain(c => c.Code == "USD");
        dataList.Should().Contain(c => c.Code == "EUR");
        
        _mockCurrencyRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllCurrenciesAsync_ShouldThrowException_WhenNoCurrenciesExist()
    {
        // Arrange: Setup empty currency list
        _mockCurrencyRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Currency>());

        // Act & Assert: Verify exception is thrown
        await Assert.ThrowsAsync<EntityNotFoundException<Currency>>(
            async () => await _rateService.GetAllCurrenciesAsync());
    }

    [Fact]
    public async Task GetDetailsAsync_ShouldReturnFailure_WhenNoRatesFound()
    {
        // Arrange: Setup currency without rates
        var currency = new Currency 
        { 
            Id = Guid.NewGuid(), 
            R030 = 840, 
            Code = "USD", 
            Name = "US Dollar" 
        };
        
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        _mockCurrencyRepository
            .Setup(x => x.GetByCodeAsync("USD"))
            .ReturnsAsync(currency);

        _mockRateRepository
            .Setup(x => x.GetRangeAsync(startDate, endDate, currency))
            .ReturnsAsync(new List<Rate>());

        // Act: Call the service method
        var result = await _rateService.GetDetailsAsync("USD", startDate, endDate);

        // Assert: Verify failure result
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("No rates found for the specified currency and date range.");
        
        _mockCurrencyRepository.Verify(x => x.GetByCodeAsync("USD"), Times.Once);
        _mockRateRepository.Verify(x => x.GetRangeAsync(startDate, endDate, currency), Times.Once);
    }
}

