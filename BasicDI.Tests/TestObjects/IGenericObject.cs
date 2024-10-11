namespace BasicDI.TestObjects;

public interface IGenericObject<T>
{
    public T? Value
    {
        get; set;
    }
}
