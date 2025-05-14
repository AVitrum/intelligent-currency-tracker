using Domain.Common;
using Shared.Dtos;

namespace Infrastructure.Identity.Results;

public class GetSettingsResult : BaseResult
{
    protected GetSettingsResult(bool success, IEnumerable<string> errors, SettingsDto settings) : base(success, errors)
    {
        Settings = settings;
    }

    public SettingsDto Settings { get; set; }

    public static GetSettingsResult SuccessResult(SettingsDto settings)
    {
        return new GetSettingsResult(true, [], settings);
    }
}