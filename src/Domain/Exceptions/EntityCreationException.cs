namespace Domain.Exceptions;

public class EntityCreationException<T>() : Exception($"Entity {nameof(T)} creation failed.");