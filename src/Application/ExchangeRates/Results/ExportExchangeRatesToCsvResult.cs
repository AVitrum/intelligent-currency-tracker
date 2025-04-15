using Domain.Common;

namespace Application.ExchangeRates.Results;

public class ExportExchangeRatesToCsvResult : BaseResult
{
    private ExportExchangeRatesToCsvResult(
        bool success,
        IEnumerable<string> errors,
        byte[] fileContent,
        string fileName) : base(success, errors)
    {
        FileContent = fileContent;
        FileName = fileName;
    }

    public byte[] FileContent { get; }
    public string FileName { get; }

    public static ExportExchangeRatesToCsvResult SuccessResult(byte[] fileContent, string fileName)
    {
        return new ExportExchangeRatesToCsvResult(true, Array.Empty<string>(), fileContent, fileName);
    }
}