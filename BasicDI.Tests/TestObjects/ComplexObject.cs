namespace BasicDI.TestObjects;

public class ComplexObject(IGenericObject<ISimpleObject> genericObject, IOtherObject otherObject, ISimpleObject simpleObject) : IComplexObject
{
    public IGenericObject<ISimpleObject> GenericObject { get; set; } = genericObject;
    public IOtherObject OtherObject { get; set; } = otherObject;
    public ISimpleObject SimpleObject { get; set; } = simpleObject;
}