using Shared.Dtos;

namespace Shared.Payload.Responses;

public record GetAllUsersResponse(IEnumerable<UserDto> Users);