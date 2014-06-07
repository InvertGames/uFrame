using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UBViewModelProperty : UBVariable<ModelPropertyBase>
{
    private string _name1;

    [SerializeField]
    private UBVarType _varType = UBVarType.Object;

    public override string Guid
    {
        get
        {
            return _name1;
        }
    }

    public override object LiteralObjectValue
    {
        get
        {
            return Property.ObjectValue;
        }
        set
        {
            Property.QuietlySetValue(value);
        }
    }

    public override string Name
    {
        get { return _name1; }
        set { _name1 = value; }
    }

    public ModelPropertyBase Property { get; set; }

    public override Type ValueType
    {
        get { return Property.ValueType; }
    }

    public override UBVarType VarType
    {
        get { return _varType; }
        set { _varType = value; }
    }

    public UBViewModelProperty(string name, ModelPropertyBase property)
        : base(false)
    {
        _name1 = name;
        Property = property;
    }

    public override void DeserializeValue(IReferenceHolder referenceHolder, UBBinarySerializer serializer)
    {
    }

    public override string ToString(IUBehaviours behaviour)
    {
        return string.Format("%{0}%", _name1);
    }

    public override void SerializeValue(IReferenceHolder referenceHolder, UBBinarySerializer serializer)
    {
    }

    protected override IEnumerable<ValueFromInfo> FillValueFromOptions(IUBVariableDeclare[] contextVariables)
    {
        yield return new ValueFromInfo(0, "Specified");
        yield return new ValueFromInfo(1, "Variable",_ValueFromVariableName);
    }
}