using Domain.Common;
using Shared.Payload.Responses;

namespace Infrastructure.Identity.Results;

public class ProfileResult : BaseResult
{
    public ProfileResponse Profile { get; }
    
    private ProfileResult(bool success, IEnumerable<string> errors, ProfileResponse profile) : base(success, errors)
    {
        Profile = profile;
    }
    
    public static ProfileResult SuccessResult(ProfileResponse profile) => new ProfileResult(true, [], profile);
}