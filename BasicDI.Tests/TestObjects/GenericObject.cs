namespace BasicDI.TestObjects;

public class GenericObject<T> : IGenericObject<T>
{
    public T Value
    {
        get; set;
    }
}
