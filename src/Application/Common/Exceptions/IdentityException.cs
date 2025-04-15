namespace Application.Common.Exceptions;

public class IdentityException(List<string> errors) : Exception(string.Join(", ", errors));