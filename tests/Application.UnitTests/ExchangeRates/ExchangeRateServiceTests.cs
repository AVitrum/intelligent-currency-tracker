// using System.Globalization;
// using Application.Common.Interfaces;
// using Application.Common.Payload.Dtos;
// using Application.Common.Payload.Requests;
// using Application.ExchangeRates;
// using Application.ExchangeRates.Results;
// using AutoMapper;
// using Confluent.Kafka;
// using Domain.Common;
// using Domain.Entities;
// using FakeItEasy;
// using Microsoft.Extensions.Logging;
//
// namespace Application.UnitTests.ExchangeRates;
//
// public class ExchangeRateServiceTests
// {
//     private readonly IExchangeRateRepository _exchangeRateRepository;
//     private readonly ILogger<ExchangeRateService> _logger;
//     private readonly IMapper _mapper;
//     private readonly IAppSettings _appSettings;
//     private readonly IKafkaProducer _kafkaProducer;
//     private readonly ExchangeRateService _exchangeRateService;
//
//     public ExchangeRateServiceTests()
//     {
//         _exchangeRateRepository = A.Fake<IExchangeRateRepository>();
//         _logger = A.Fake<ILogger<ExchangeRateService>>();
//         _mapper = A.Fake<IMapper>();
//         _appSettings = A.Fake<IAppSettings>();
//         _kafkaProducer = A.Fake<IKafkaProducer>();
//
//         _exchangeRateService = new ExchangeRateService(_exchangeRateRepository, _logger, _mapper, _appSettings, _kafkaProducer);
//     }
//
//     [Fact]
//     public async Task GetRangeAsync_ShouldReturnSuccessResult_WhenAllDataIsRight()
//     {
//         // Arrange
//         var dto = new ExchangeRateRequest
//         {
//             Currency = "USD",
//             StartDateString = "01.01.2022",
//             EndDateString = "02.01.2022"
//         };
//
//         DateTime.TryParseExact(dto.StartDateString, "dd.MM.yyyy", null, DateTimeStyles.None, out DateTime startDate);
//         DateTime.TryParseExact(dto.EndDateString, "dd.MM.yyyy", null, DateTimeStyles.None, out DateTime endDate);
//         dto.Start = startDate;
//         dto.End = endDate;
//
//         var exchangeRates = new List<ExchangeRate>
//         {
//             new()
//             {
//                 Currency = "USD",
//                 SaleRateNb = 1.0m,
//                 PurchaseRateNb = 1.0m,
//                 Date = DateTime.UtcNow
//             }
//         };
//
//         var exchangeRateDto = new ExchangeRateDto
//         {
//             Currency = "USD",
//             Date = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
//             SaleRateNb = 1.0m,
//             PurchaseRateNb = 1.0m,
//             SaleRate = 0,
//             PurchaseRate = 0
//         };
//
//         A.CallTo(() => _exchangeRateRepository.GetAllByStartDateAndEndDateAndCurrencyAsync(dto.Start, dto.End, dto.Currency))
//             .Returns(exchangeRates);
//         A.CallTo(() => _mapper.Map<ExchangeRateDto>(A<ExchangeRate>.Ignored)).Returns(exchangeRateDto);
//
//         // Act
//         var result = (GetExchangeRateRangeResult)await _exchangeRateService.GetRangeAsync(dto);
//
//         // Assert
//         Assert.True(result.Success);
//         Assert.NotNull(result.Data);
//         A.CallTo(() => _kafkaProducer.ProduceAsync("exchange-rates", A<Message<string, string>>.Ignored)).MustHaveHappenedOnceExactly();
//     }
//
//     [Fact]
//     public async Task GetRangeAsync_ShouldReturnFailureResult_WhenNoExchangeRatesFound()
//     {
//         // Arrange
//         var dto = new ExchangeRateRequest
//         {
//             StartDateString = "01.01.2022",
//             EndDateString = "02.01.2022",
//             Currency = "USD"
//         };
//
//         DateTime.TryParseExact(dto.StartDateString, "dd.MM.yyyy", null, DateTimeStyles.None, out DateTime startDate);
//         DateTime.TryParseExact(dto.EndDateString, "dd.MM.yyyy", null, DateTimeStyles.None, out DateTime endDate);
//         dto.Start = startDate;
//         dto.End = endDate;
//
//         A.CallTo(() => _exchangeRateRepository.GetAllByStartDateAndEndDateAndCurrencyAsync(dto.Start, dto.End, dto.Currency))
//             .Returns(new List<ExchangeRate>());
//
//         // Act
//         BaseResult result = await _exchangeRateService.GetRangeAsync(dto);
//
//         // Assert
//         Assert.False(result.Success);
//         Assert.Contains("No exchange rates found", result.Errors);
//     }
// }

