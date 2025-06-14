using Domain.Common;

namespace Application.Reports.Results;

public class EmailResult : BaseResult
{
    public string Email { get; private set; }

    private EmailResult(bool success, IEnumerable<string> errors, string email) : base(success, errors)
    {
        Email = email;
    }

    public static EmailResult SuccessResult(string email)
    {
        return new EmailResult(true, [], email);
    }
}