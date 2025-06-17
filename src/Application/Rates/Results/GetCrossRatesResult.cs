    using Domain.Common; 
    using Shared.Dtos;   

    namespace Application.Rates.Results;
    
    public class GetCrossRatesResult : BaseResult
    {
        public IEnumerable<CrossRateDto> CrossRates { get; }
    
        private GetCrossRatesResult(bool succeeded, IEnumerable<string> errors, IEnumerable<CrossRateDto> crossRates)
            : base(succeeded, errors)
        {
            CrossRates = crossRates;
        }
    
        public static GetCrossRatesResult SuccessResult(IEnumerable<CrossRateDto> crossRates)
        {
            return new GetCrossRatesResult(true, [], crossRates);
        }
    }