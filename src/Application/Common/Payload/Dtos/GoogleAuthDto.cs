using Application.Common.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authentication;

namespace Application.Common.Payload.Dtos;

public class GoogleAuthDto : IUserRequest
{ 
    public required AuthenticateResult AuthenticateResult { get; init; }
    
    public UserServiceType ServiceType { get; set; } = UserServiceType.GOOGLE;
}