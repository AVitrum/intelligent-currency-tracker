namespace Domain.Exceptions;

public class EntityNotFoundException<T>() : Exception($"{nameof(T)} not found");