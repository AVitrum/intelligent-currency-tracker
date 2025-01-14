namespace Application.Common.Exceptions;

public class WrongTypeRequestException(string requiredExceptionName, string receivedException)
    : Exception($"Expected {requiredExceptionName} but received {receivedException}");