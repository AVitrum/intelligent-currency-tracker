using Domain.Enums;

namespace Shared.Dtos;

public record MovingAverageDto(int Period, decimal Value, MovingAverageType Type);