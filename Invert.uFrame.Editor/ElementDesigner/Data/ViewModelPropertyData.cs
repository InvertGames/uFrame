using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Invert.uFrame.Editor.Refactoring;
using UnityEngine;

[Serializable]
public class ViewModelPropertyData : DiagramSubItem,IViewModelItem
{
    [SerializeField]
    private string _type;

    [SerializeField]
    private bool _isRealTimeProperty;

    public Type Type
    {
        get
        {
            if (_type == null)
            {
                return typeof(string);
            }
            return Type.GetType(_type);
        }
        set { _type = value.AssemblyQualifiedName; }
    }

    public object DefaultValue { get; set; }

    public string NameAsChangedMethod
    {
        get { return string.Format("{0}Changed", Name); }
    }
    public string NameAsTwoWayMethod
    {
        get { return string.Format("Get{0}TwoWayValue", Name); }
    }
    public override string Label
    {
        get { return RelatedTypeName + ": " + Name; }
    }

    public override RenameRefactorer CreateRenameRefactorer()
    {
        return new RenamePropertyRefactorer(this);
    }

    public override void CreateLink(IDiagramItem container, IDrawable target)
    {
        var element = target as IDiagramItem;
        if (element != null)
        {
            RelatedType = element.AssemblyQualifiedName;
        }
    }

    public override bool CanCreateLink(IDrawable target)
    {
        return target is ElementDataBase || target is EnumData;
    }

    public override void RemoveLink(IDiagramItem target)
    {
        RelatedType = typeof (string).AssemblyQualifiedName;
    }

    public string RelatedType
    {
        get { return _type; }
        set { _type = value; }
    }

    public bool IsRealTimeProperty
    {
        get { return _isRealTimeProperty; }
        set { _isRealTimeProperty = value; }
    }

    public string RelatedTypeName
    {
        get
        {

            return RelatedType.Split(',').FirstOrDefault() ?? "No Type";
        }
    }

    public bool AllowEmptyRelatedType
    {
        get { return false; }
    }

    public string FieldName
    {
        get
        {
            return string.Format("_{0}Property", Name);
        }
    }

    public string ViewFieldName
    {
        get
        {
            return string.Format("_{0}", Name);
        }
    }

    public override IEnumerable<IDiagramLink> GetLinks(IDiagramItem[] data)
    {
        foreach (var viewModelData in data)
        {
            if (viewModelData.Name == null) continue;
            if (viewModelData.Name == RelatedTypeName)
            {
                yield return new AssociationLink()
                {
                    Item = this,
                    Element = viewModelData
                };
            }
        }
    }

    public override string Highlighter
    {
        get { return null; }
    }

    public override string FullLabel { get { return RelatedTypeName + Name; } }
    
    public override bool IsSelectable { get { return true; } }

    public override void Remove(IDiagramItem diagramItem)
    {
        var data = diagramItem as ElementDataBase;
        data.Properties.Remove(this);
        data.Dirty = true;
    }


    public string NameAsPrefabBindingOption
    {
        get { return string.Format("_{0}Prefab", Name); }
    }

    public string NameAsTwoWayBindingOption
    {
        get { return string.Format("_{0}IsTwoWay", Name); }
    }
    public string NameAsBindingOption
    {
        get { return string.Format("_Bind{0}", Name); }
    }
}