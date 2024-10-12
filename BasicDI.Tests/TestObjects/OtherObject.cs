namespace BasicDI.TestObjects;

public class OtherObject(ISimpleObject simpleObject) : IOtherObject
{
    public ISimpleObject SimpleObject { get; set; } = simpleObject;
}