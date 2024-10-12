namespace BasicDI.TestObjects;

public class MultipleConstructors : IMultipleConstructors
{
    public MultipleConstructors()
    {
    }

    public MultipleConstructors(ISimpleObject simpleObject)
    {
        SimpleObject = simpleObject;
    }

    public MultipleConstructors(IOtherObject otherObject, ISimpleObject simpleObject)
    {
        OtherObject = otherObject;
        SimpleObject = simpleObject;
    }

    public IOtherObject? OtherObject
    {
        get; set;
    }

    public ISimpleObject? SimpleObject
    {
        get; set;
    }
}
