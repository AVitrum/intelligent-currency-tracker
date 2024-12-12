using Domain.Common;

namespace Application.ExchangeRates.Results;

public class ExportExchangeRatesToCsvResult : BaseResult
{
    public byte[] FileContent { get; }
    public string FileName { get; }
    
    private ExportExchangeRatesToCsvResult(bool success, IEnumerable<string> errors, byte[] fileContent, string fileName) : base(success, errors)
    {
        FileContent = fileContent;
        FileName = fileName;
    }
    
    public static ExportExchangeRatesToCsvResult SuccessResult(byte[] fileContent, string fileName) =>
        new(true, Array.Empty<string>(), fileContent, fileName);
}