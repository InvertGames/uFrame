using UnityEngine;
[UBCategory(" UBehaviours")]
public class BoolCheck : UBConditionAction
{
    [UBRequired]
    public UBBool _BoolValue = new UBBool();

    public override bool PerformCondition(IUBContext context)
    {
        return _BoolValue.GetValue(context);
    }

    public override string ToString()
    {
        return string.Format("{0} is true?", _BoolValue.ToString(RootContainer));
    }
}
[UBCategory(" UBehaviours")]
public class IsSet : UBConditionAction
{
    [UBRequired]
    public UBObject _Object = new UBObject();

    public override bool PerformCondition(IUBContext context)
    {
        return _Object.GetValue(context) != null;
    }

    public override string ToString()
    {
        return string.Format("Is {0} Set?", _Object.ToString(RootContainer));
    }
}