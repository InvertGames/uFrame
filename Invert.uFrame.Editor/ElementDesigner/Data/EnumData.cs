using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class EnumData : DiagramItem
{
    [SerializeField]
    private List<EnumItem> _enumItems = new List<EnumItem>();

    public List<EnumItem> EnumItems
    {
        get { return _enumItems; }
        set { _enumItems = value; }
    }

    public override void EndEditing()
    {
        base.EndEditing();
        foreach (var item in Data.ViewModels.SelectMany(p => p.Properties).Where(p => p.RelatedTypeName == OldName))
        {
            item.RelatedType = AssemblyQualifiedName;
        }
        foreach (var item in Data.ViewModels.SelectMany(p => p.Commands).Where(p => p.RelatedTypeName == OldName))
        {
            item.RelatedType = AssemblyQualifiedName;
        }
        foreach (var item in Data.ViewModels.SelectMany(p => p.Collections).Where(p => p.RelatedTypeName == OldName))
        {
            item.RelatedType = AssemblyQualifiedName;
        }
    }

  
    public override void RemoveFromDiagram()
    {
        Data.Enums.Remove(this);
    }


    public override string InfoLabel
    {
        get { return null; }
    }

    public override string Label { get { return Name; }}

    public override IEnumerable<IDiagramSubItem> Items
    {
        get { return EnumItems.Cast<IDiagramSubItem>(); }
    }

    public override void CreateLink(IDiagramItem container, IDrawable target)
    {
        
    }

    public override bool CanCreateLink(IDrawable target)
    {
        return false;
    }

    public override IEnumerable<IDiagramLink> GetLinks(IDiagramItem[] elementDesignerData)
    {
        yield break;
    }


    public override void RemoveLink(IDiagramItem target)
    {
        
    }
}