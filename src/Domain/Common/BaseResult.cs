namespace Domain.Common;

public class BaseResult
{
    public bool Success { get; init; }
    public string[] Errors { get; set; }
    
    protected BaseResult(bool success, IEnumerable<string> errors)
    {
        Success = success;
        Errors = errors.ToArray();
    }
    
    public static BaseResult SuccessResult() => new(true, Array.Empty<string>());
    public static BaseResult FailureResult(IEnumerable<string> errors) => new(false, errors);
}