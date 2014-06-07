using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor.Refactoring;
using UnityEngine;


[Serializable]
public class ViewModelCollectionData : DiagramSubItem, IViewModelItem
{
    public bool AllowEmptyRelatedType
    {
        get { return false; }
    }

    [SerializeField]
    private string _itemType;

    public Type ItemType
    {
        get { return Type.GetType(_itemType); }
        set { _itemType = value.AssemblyQualifiedName; }
    }

    public override string Label
    {
        get { return RelatedTypeName + "[]: " + Name; }
    }
    public override RenameRefactorer CreateRenameRefactorer()
    {
        return new RenameCollectionRefactorer(this);
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
        RelatedType = typeof(string).AssemblyQualifiedName;
    }

    public string RelatedType
    {
        get { return _itemType; }
        set { _itemType = value; }
    }

    public string RelatedTypeName
    {
        get
        {
            return RelatedType.Split(',').FirstOrDefault() ?? "No Type";
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

    public override string FullLabel { get { return RelatedTypeName + Name; } }

    public override void Remove(IDiagramItem diagramItem)
    {
        var data = diagramItem as ElementDataBase;
        data.Collections.Remove(this);
        data.Dirty = true;
    }


    public string FieldName
    {
        get
        {
            return string.Format("_{0}Property", Name);
        }
    }

    public string NameAsCreateHandler
    {
        get { return string.Format("Create{0}View", Name); }
    }
    public string NameAsAddHandler
    {
        get { return string.Format("{0}Added", Name); }
    }

    public string NameAsContainerBindingOption
    {
        get { return string.Format("_{0}Prefab", Name); }
    }
    public string NameAsSceneFirstBindingOption
    {
        get { return string.Format("_{0}SceneFirst", Name); }
    }
    public string NameAsUseArrayBindingOption
    {
        get { return string.Format("_{0}UseArray", Name); }
    }
    public string NameAsListBindingOption
    {
        get { return string.Format("_{0}List", Name); }
    }
    public string NameAsBindingOption
    {
        get { return string.Format("_Bind{0}", Name); }
    }
}