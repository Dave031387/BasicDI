namespace BasicDI.TestObjects;

public class ConstructorException : ISimpleObject
{
    public ConstructorException() => throw new NotImplementedException();
}
