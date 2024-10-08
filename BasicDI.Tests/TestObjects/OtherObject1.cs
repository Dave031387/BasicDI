namespace BasicDI.TestObjects;

public class OtherObject1(ISimpleObject simpleObject) : IOtherObject
{
    public ISimpleObject SimpleObject { get; set; } = simpleObject;
}