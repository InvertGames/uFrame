using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class SceneManagerTransition : IDiagramSubItem
{
    [SerializeField]
    private string _toIdentifier;
    [SerializeField]
    private string _name;

    [SerializeField]
    private string _fromCommand;

    public string CommandIdentifier
    {
        get { return _fromCommand; }
        set { _fromCommand = value; }
    }

    public string ToIdentifier
    {
        get { return _toIdentifier; }
        set { _toIdentifier = value; }
    }

    public Rect Position { get; set; }

    public string Label
    {
        get { return Name; }
    }

    public void CreateLink(IDiagramItem container, IDrawable target)
    {
        ToIdentifier = ((SceneManagerData) target).Identifier;
    }

    public bool CanCreateLink(IDrawable target)
    {
        return target is SceneManagerData;
    }

    public IEnumerable<IDiagramLink> GetLinks(IDiagramItem[] elementDesignerData)
    {
        //elementDesignerData.OfType<ElementDataBase>().SelectMany(p=>p.Commands).FirstOrDefault(p=>p.n)
        yield break;
        //var linkedTo = elementDesignerData.OfType<SceneManagerData>().FirstOrDefault(p => p.Name == ToIdentifier);
        //if (linkedTo == null) yield break;

        //yield return new TransitionLink()
        //{
        //    From = this,
        //    To = linkedTo
        //};
    }

    public bool IsSelected { get; set; }
    public void RemoveLink(IDiagramItem target)
    {
        ToIdentifier = null;
    }

    public string Name
    {
        get { return _name; }
        set { _name = Regex.Replace(value, "[^a-zA-Z0-9_.]+", ""); }
    }

    public string Highlighter { get; private set; }
    public string FullLabel { get { return Name; } }
    [SerializeField]
    private string _identifier;
    public string Identifier{ get { return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier;}}
    public bool IsSelectable { get { return false; } }

    public string NameAsSettingsField
    {
        get { return string.Format("_{0}Transition", Name); }
    }

    public void Remove(IDiagramItem data)
    {
        //var SceneManagerData = data as SceneManagerData;
        //if (SceneManagerData != null) 
        //    SceneManagerData.Transitions.Remove(this);
    }

    public void Rename(IDiagramItem data, string name)
    {
        Name = name;
    }

    public Vector2[] ConnectionPoints { get; set; }
}

