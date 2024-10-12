namespace BasicDI.TestObjects;

public interface IMultipleConstructors
{
    IOtherObject? OtherObject
    {
        get; set;
    }

    ISimpleObject? SimpleObject
    {
        get; set;
    }
}
