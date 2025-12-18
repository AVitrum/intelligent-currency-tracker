using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests;

public class RateRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly RateRepository _repository;
    private readonly Currency _testCurrency;
    private readonly Guid _testCurrencyId;

    public RateRepositoryTests()
    {
        // Arrange: Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new RateRepository(_context);
        
        _testCurrencyId = Guid.NewGuid();
        _testCurrency = new Currency
        {
            Id = _testCurrencyId,
            R030 = 840,
            Code = "USD",
            Name = "US Dollar"
        };
        _context.Currencies.Add(_testCurrency);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetLastByCurrencyIdAsync_ShouldReturnLatestRate_WhenRatesExist()
    {
        // Arrange: Add test rates
        var oldRate = new Rate
        {
            Id = Guid.NewGuid(),
            CurrencyId = _testCurrencyId,
            Value = 27.5m,
            ValueCompareToPrevious = 0,
            Date = DateTime.UtcNow.AddDays(-2)
        };
        var latestRate = new Rate
        {
            Id = Guid.NewGuid(),
            CurrencyId = _testCurrencyId,
            Value = 28.0m,
            ValueCompareToPrevious = 0.5m,
            Date = DateTime.UtcNow.AddDays(-1)
        };

        await _context.Rates.AddRangeAsync(oldRate, latestRate);
        await _context.SaveChangesAsync();

        // Act: Get the latest rate
        var result = await _repository.GetLastByCurrencyIdAsync(_testCurrencyId);

        // Assert: Verify the latest rate is returned
        result.Should().NotBeNull();
        result.Id.Should().Be(latestRate.Id);
        result.Value.Should().Be(28.0m);
        result.Date.Should().BeCloseTo(latestRate.Date, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetLastByCurrencyIdAsync_ShouldReturnNull_WhenNoRatesExist()
    {
        // Arrange: Use a currency ID with no rates
        var nonExistentCurrencyId = Guid.NewGuid();

        // Act
        var result = await _repository.GetLastByCurrencyIdAsync(nonExistentCurrencyId);

        // Assert: Verify null is returned
        Assert.Null(result);
    }

    [Fact]
    public async Task GetRangeAsync_ShouldReturnRatesInDateRange_WithPagination()
    {
        // Arrange: Add multiple test rates
        var startDate = DateTime.UtcNow.AddDays(-10);
        var endDate = DateTime.UtcNow.AddDays(-1);
        
        var rates = new List<Rate>();
        for (var i = 0; i < 15; i++)
        {
            rates.Add(new Rate
            {
                Id = Guid.NewGuid(),
                CurrencyId = _testCurrencyId,
                Value = 27.0m + i * 0.1m,
                ValueCompareToPrevious = 0.1m,
                Date = startDate.AddDays(i)
            });
        }

        await _context.Rates.AddRangeAsync(rates);
        await _context.SaveChangesAsync();

        // Act: Get rates with pagination (page 1, 5 items per page)
        var result = await _repository.GetRangeAsync(startDate, endDate, page: 1, pageSize: 5);

        // Assert: Verify pagination works correctly
        var resultList = result.ToList();
        resultList.Should().HaveCount(5);
        resultList.First().Value.Should().Be(27.0m);
        resultList.Last().Value.Should().Be(27.4m);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleRates_WhenValidRatesProvided()
    {
        // Arrange: Create a list of rates to add
        var ratesToAdd = new List<Rate>
        {
            new Rate
            {
                Id = Guid.NewGuid(),
                CurrencyId = _testCurrencyId,
                Value = 30.0m,
                ValueCompareToPrevious = 0,
                Date = DateTime.UtcNow.AddDays(-3)
            },
            new Rate
            {
                Id = Guid.NewGuid(),
                CurrencyId = _testCurrencyId,
                Value = 30.5m,
                ValueCompareToPrevious = 0.5m,
                Date = DateTime.UtcNow.AddDays(-2)
            }
        };

        // Act: Add rates using repository
        await _repository.AddRangeAsync(ratesToAdd);

        // Assert: Verify rates were added to database
        var allRates = await _context.Rates.ToListAsync();
        allRates.Should().HaveCount(2);
        allRates.Should().Contain(r => r.Value == 30.0m);
        allRates.Should().Contain(r => r.Value == 30.5m);
    }

    [Fact]
    public async Task RemoveByDateAsync_ShouldRemoveRates_WhenRatesExistForDate()
    {
        // Arrange: Add rates for a specific date
        var targetDate = DateTime.UtcNow.Date;
        var rateToRemove = new Rate
        {
            Id = Guid.NewGuid(),
            CurrencyId = _testCurrencyId,
            Value = 28.5m,
            ValueCompareToPrevious = 0,
            Date = targetDate
        };
        
        await _context.Rates.AddAsync(rateToRemove);
        await _context.SaveChangesAsync();

        // Act: Remove rates by date
        var result = await _repository.RemoveByDateAsync(targetDate);

        // Assert: Verify rates were removed
        result.Should().BeTrue();
        var remainingRates = await _context.Rates.Where(r => r.Date.Date == targetDate).ToListAsync();
        remainingRates.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveByDateAsync_ShouldReturnFalse_WhenNoRatesExistForDate()
    {
        // Arrange: Use a date with no rates
        var nonExistentDate = DateTime.UtcNow.AddYears(-10);

        // Act: Try to remove rates for non-existent date
        var result = await _repository.RemoveByDateAsync(nonExistentDate);

        // Assert: Verify false is returned
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

