namespace Application.Common.Exceptions;

public class UserNotFoundException(string message) : Exception(message);