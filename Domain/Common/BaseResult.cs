namespace Domain.Common;

public abstract class BaseResult
{
    public bool Success { get; init; }
    public string[] Errors { get; set; }
    
    protected BaseResult(bool success, IEnumerable<string> errors)
    {
        Success = success;
        Errors = errors.ToArray();
    }
}