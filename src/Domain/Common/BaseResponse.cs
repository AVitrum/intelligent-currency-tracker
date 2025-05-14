namespace Domain.Common;

public abstract class BaseResponse
{
    protected BaseResponse(bool success, string message, int statusCode, IEnumerable<string> errors)
    {
        Success = success;
        Message = message;
        StatusCode = statusCode;
        Errors = errors;
    }

    public bool Success { get; set; }
    public string Message { get; set; }
    public int StatusCode { get; set; }
    public IEnumerable<string> Errors { get; set; }
}