using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Common.Payload.Dtos;
using Domain.Common;
using Domain.Enums;
using Infrastructure.ExternalApis.GoogleAuth.Results;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.ExternalApis.GoogleAuth;

public class GoogleUserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public GoogleUserService(UserManager<ApplicationUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<BaseResult> CreateUserAsync(IUserRequest request)
    {
        if (request is not GoogleAuthDto dto)
        {
            throw new WrongTypeRequestException(nameof(dto), nameof(request));
        }
        
        AuthenticateResult authResult = dto.AuthenticateResult;
        if (!authResult.Succeeded)
        {
            return GoogleAuthResult.FailureResult(new List<string> {"Authentication failed"});
        }
        
        string? email = authResult.Principal?.FindFirst(ClaimTypes.Email)?.Value;
        if (email is null)
        {
            return GoogleAuthResult.FailureResult(["Email claim not found"]);
        }
        
        ApplicationUser? newUser = await _userManager.FindByEmailAsync(email);
        if (newUser is not null)
        {
            return await LoginAsync(new GoogleLoginDto(newUser));
        }
        
        newUser = new ApplicationUser
        {
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            NormalizedEmail = email.ToUpper(),
            NormalizedUserName = email.ToUpper(),
            CreationMethod = UserCreationMethod.GOOGLE
        };
        
        IdentityResult result = await _userManager.CreateAsync(newUser);
        if (!result.Succeeded)
        {
            return BaseResult.FailureResult(result.Errors.Select(error => error.Description).ToList());
        }
        
        IdentityResult addToRoleResult = await _userManager.AddToRoleAsync(newUser, UserRole.User.ToString());
        if (!addToRoleResult.Succeeded)
        {
            return BaseResult.FailureResult(addToRoleResult.Errors.Select(error => error.Description).ToList());
        }
        return await LoginAsync(new GoogleLoginDto(newUser));
    }

    public Task<BaseResult> LoginAsync(IUserRequest request)
    {
        if (request is not GoogleLoginDto dto)
        {
            throw new WrongTypeRequestException(nameof(dto), nameof(request));
        }
        ApplicationUser user = dto.ApplicationUser;
        
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];
        JwtSecurityToken token = _jwtService.GenerateToken(claims);
        return Task.FromResult<BaseResult>(GoogleAuthResult.SuccessResult(new JwtSecurityTokenHandler().WriteToken(token)));
    }
    
    private record GoogleLoginDto(ApplicationUser ApplicationUser) : IUserRequest
    {
        public UserServiceType ServiceType { get; set; } = UserServiceType.GOOGLE;
    }
}