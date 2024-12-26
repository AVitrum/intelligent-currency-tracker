namespace Domain.Exceptions;

public class EntityNotFoundException<T> : Exception
{
    public EntityNotFoundException() : base($"{typeof(T).Name} not found") { }
}