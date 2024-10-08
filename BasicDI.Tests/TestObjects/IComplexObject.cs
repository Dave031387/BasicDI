namespace BasicDI.TestObjects;

public interface IComplexObject
{
    ISimpleObject SimpleObject
    {
        get;
        set;
    }
    IOtherObject OtherObject
    {
        get;
        set;
    }
    IGenericObject<ISimpleObject> GenericObject
    {
        get;
        set;
    }
}