using Domain.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity;

public class UserFactory
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserFactory> _logger;
    public delegate ApplicationUser UserFactoryDelegate();
    public delegate Task<IdentityResult> PostCreationDelegate(ApplicationUser user);

    public UserFactory(UserManager<ApplicationUser> userManager, ILogger<UserFactory> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<BaseResult> CreateUserAsync(UserFactoryDelegate userFactory, 
        PostCreationDelegate? postCreationDelegate = null)
    {
        ApplicationUser user = userFactory();
    
        _logger.LogInformation("Creating user with email: {Email}", user.Email);
        IdentityResult creationResult = await _userManager.CreateAsync(user);
        if (!creationResult.Succeeded)
        {
            var errors = creationResult.Errors.Select(error => error.Description).ToList();
            _logger.LogError("User creation failed: {Errors}", string.Join(", ", errors));
            return BaseResult.FailureResult(errors);
        }
    
        if (postCreationDelegate is not null)
        {
            _logger.LogInformation("Executing post-creation action for user: {Email}", user.Email);
            IdentityResult postActionResult = await postCreationDelegate(user);
            if (!postActionResult.Succeeded)
            {
                var errors = postActionResult.Errors.Select(error => error.Description).ToList();
                _logger.LogError("Post-creation action failed: {Errors}", string.Join(", ", errors));
                return BaseResult.FailureResult(errors);
            }
        }

        _logger.LogInformation("User created successfully: {Email}", user.Email);
        return BaseResult.SuccessResult();
    }
}