namespace Domain.Exceptions;

public class EntityNotFoundException<T>() : Exception($"{typeof(T)} not found");