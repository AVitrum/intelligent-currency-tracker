namespace Domain.Common;

public class BaseResult
{
    protected BaseResult(bool success, IEnumerable<string> errors)
    {
        Success = success;
        Errors = errors.ToArray();
    }

    public bool Success { get; }

    public string[] Errors { get; }

    public static BaseResult SuccessResult()
    {
        return new BaseResult(true, []);
    }

    public static BaseResult FailureResult(IEnumerable<string> errors)
    {
        return new BaseResult(false, errors);
    }
}