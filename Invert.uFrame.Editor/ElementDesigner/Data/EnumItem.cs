using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnumItem : IDiagramSubItem
{
    [SerializeField]
    private string _name;

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public string Highlighter
    {
        get { return null; }
    }

    public string FullLabel { get { return  Name; } }
    public string Identifier{ get { return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier;}}
    public bool IsSelectable { get { return true; } }

    [SerializeField]
    private string _identifier;
    public void Remove(IDiagramItem diagramItem)
    {
        var data = diagramItem as EnumData;
        data.EnumItems.Remove(this);
        data.Dirty = true;
    }

    public void Rename(IDiagramItem data, string name)
    {
        Name = name;
    }

    public Vector2[] ConnectionPoints { get; set; }

    public Rect Position { get; set; }

    public string Label
    {
        get { return Name; }
    }

    public void CreateLink(IDiagramItem container, IDrawable target)
    {
        
    }

    public bool CanCreateLink(IDrawable target)
    {
        return false;
    }

    public IEnumerable<IDiagramLink> GetLinks(IDiagramItem[] elementDesignerData)
    {
        yield break;
    }

    public bool IsSelected { get; set; }
    public void RemoveLink(IDiagramItem target)
    {
        throw new NotImplementedException();
    }
}