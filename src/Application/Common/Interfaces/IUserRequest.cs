using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IUserRequest
{
    UserServiceType ServiceType { get; set; }
}