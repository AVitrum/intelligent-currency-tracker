namespace Application.Common.Exceptions;

public class WrongDateException(string requiredDate, string receivedDate)
    : Exception($"Required date: {requiredDate}, received date: {receivedDate}");