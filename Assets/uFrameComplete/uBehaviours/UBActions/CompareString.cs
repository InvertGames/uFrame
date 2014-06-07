using System;
using UnityEngine;

[UBCategory(" UBehaviours")]
public class CompareEnum : UBConditionAction
{
    [UBRequireVariable,UBRequired]
    public UBEnum _UbEnumA = new UBEnum();

    [ValueTypeFrom("_UbEnumA")]
    [UBRequired]
    public UBEnum _UbEnumB = new UBEnum();

    public override bool PerformCondition(IUBContext context)
    {
        var a = _UbEnumA.GetIntValue(context);
        var b = _UbEnumB.GetIntValue(context);
        return Equals(a, b);
    }

    public override string ToString()
    {
        return string.Format("{0} equals {1}?", _UbEnumA.ToString(RootContainer), _UbEnumB.ToString(RootContainer));
    }
}
[UBCategory(" UBehaviours")]
public class CompareInt : UBConditionAction
{
    [UBRequireVariable, UBRequired]
    public UBInt _UbIntA = new UBInt();

    [ValueTypeFrom("_UbIntA")]
    [UBRequired]
    public UBInt _UbIntB = new UBInt();

    public override bool PerformCondition(IUBContext context)
    {
        var a = _UbIntA.GetValue(context);
        var b = _UbIntB.GetValue(context);
        return Equals(a, b);
    }

    public override string ToString()
    {
        return string.Format("{0} equals {1}?", _UbIntA.ToString(RootContainer), _UbIntB.ToString(RootContainer));
    }
}
[UBCategory(" UBehaviours")]
public class CompareFloat : UBConditionAction
{
    [UBRequireVariable, UBRequired]
    public UBFloat _UbFloatA = new UBFloat();

    [ValueTypeFrom("_UbFloatA")]
    [UBRequired]
    public UBFloat _UbFloatB = new UBFloat();

    public override bool PerformCondition(IUBContext context)
    {
        var a = _UbFloatA.GetValue(context);
        var b = _UbFloatB.GetValue(context);
        return Equals(a, b);
    }

    public override string ToString()
    {
        return string.Format("{0} equals {1}?", _UbFloatA.ToString(RootContainer), _UbFloatB.ToString(RootContainer));
    }
}

[UBCategory("UBehaviours")]
public class CompareString : UBAction
{
    public bool _CaseSensitive = true;

    [HideInInspector]
    public UBActionSheet _Equal;

    [HideInInspector]
    public UBActionSheet _NotEqual;
    [UBRequired]
    public UBString _UbStringA = new UBString();
    [UBRequired]
    public UBString _UbStringB = new UBString();

    public override string ToString()
    {
        return string.Format("{0} equals {1}?", _UbStringA.ToString(RootContainer), _UbStringB.ToString(RootContainer));
    }

    protected override void PerformExecute(IUBContext context)
    {
        if (_CaseSensitive)
            if (_UbStringA.GetValue(context) == _UbStringB.GetValue(context))
            {
                if (_Equal != null) _Equal.Execute(context);
            }
            else
            {
                if (_NotEqual != null) _NotEqual.Execute(context);
            }
        else
        {
            if (_UbStringA.GetValue(context).ToUpper() == _UbStringB.GetValue(context).ToUpper())
            {
                if (_Equal != null) _Equal.Execute(context);
            }
            else
            {
                if (_NotEqual != null) _NotEqual.Execute(context);
            }
        }
    }
}