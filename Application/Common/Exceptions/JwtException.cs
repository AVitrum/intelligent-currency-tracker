namespace Application.Common.Exceptions;

public class JwtException : Exception
{
    public JwtException(string message) : base(message) { }
}