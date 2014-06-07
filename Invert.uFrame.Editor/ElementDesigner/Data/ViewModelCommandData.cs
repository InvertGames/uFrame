using System;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor.Refactoring;
using UnityEngine;

[Serializable]
public class ViewModelCommandData : DiagramSubItem, IViewModelItem
{
    [SerializeField]
    private bool _isYield;

    [SerializeField]
    private string _parameterType;

    [SerializeField]
    private string _transitionName;

    public bool AllowEmptyRelatedType
    {
        get { return true; }
    }

    public ElementData Element { get; set; }

    public string FieldName
    {
        get
        {
            return string.Format("_{0}", Name);
        }
    }

    public override string FullLabel
    {
        get { return (string.IsNullOrEmpty(RelatedTypeName) ? " [ NONE ] " : RelatedTypeName) + Name; }
    }

    public override string Highlighter
    {
        get
        {
            if (IsYield)
            {
                return "Yield";
            }
            return null;
        }
    }

    public override bool IsSelectable { get { return true; } }

    [DiagramContextMenu("Is Yield Command", 0)]
    public bool IsYield
    {
        get { return _isYield; }
        set { _isYield = value; }
    }

    public override string Label
    {
        get
        {
            if (string.IsNullOrEmpty(RelatedTypeName))
                return Name;

            return RelatedTypeName + ":" + Name;
        }
    }

    public string NameAsExecuteMethod
    {
        get { return string.Format("Execute{0}", this.Name); }
    }

    public string NameAsSettingsField
    {
        get { return string.Format("_{0}Transition", Name); }
    }

    public Type ParameterType
    {
        get
        {
            if (string.IsNullOrEmpty(_parameterType))
                return null;
            return Type.GetType(_parameterType);
        }
        set
        {
            if (value != null)
                _parameterType = value.AssemblyQualifiedName;
        }
    }

    public string RelatedType
    {
        get { return _parameterType; }
        set { _parameterType = value; }
    }

    public string RelatedTypeName
    {
        get
        {
            if (string.IsNullOrEmpty(RelatedType))
                return null;
            return RelatedType.Split(',').FirstOrDefault() ?? "No Type";
        }
    }

    public string TransitionToIdentifier
    {
        get { return _transitionName; }
        set { _transitionName = value; }
    }

    public override bool CanCreateLink(IDrawable target)
    {
        return target is ElementDataBase || target is SceneManagerData || target is EnumData;
    }

    public override void CreateLink(IDiagramItem container, IDrawable target)
    {
        var sceneManagerData = target as SceneManagerData;
        if (sceneManagerData != null)
        {
            var transition = sceneManagerData.Transitions.FirstOrDefault(p => p.CommandIdentifier == this.Identifier);
            transition.ToIdentifier = sceneManagerData.Identifier;
        }
        if (target is EnumData || target is ElementDataBase)
        {
            var element = target as IDiagramItem;
            RelatedType = element.AssemblyQualifiedName;
        }
    }

    public override RenameRefactorer CreateRenameRefactorer()
    {
        return new RenameCommandRefactorer(this);
    }


    public override IEnumerable<IDiagramLink> GetLinks(IDiagramItem[] data)
    {
        foreach (var viewModelData in data)
        {
            if (viewModelData.Name == null) continue;
            //var scd = viewModelData as SceneManagerData;
            //if (scd != null)
            //{
            //    if (TransitionToIdentifier == scd.Identifier)
            //    {
            //        yield return new TransitionLink()
            //        {
            //            From = this,
            //            To = scd
            //        };
            //    }
            //} else
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

    [DiagramContextMenu("Delete", 0)]
    public override void Remove(IDiagramItem diagramItem)
    {
        var data = diagramItem as ElementDataBase;
        if (data != null)
        {
            data.Commands.Remove(this);
            foreach (var sceneManagerData in data.Data.SceneManagers)
                sceneManagerData.Transitions.RemoveAll(p => p.CommandIdentifier == this.Identifier);
            data.Dirty = true;
        }
    }

    public override void RemoveLink(IDiagramItem target)
    {
        
        RelatedType = null;
    }

    public override void Rename(IDiagramItem data, string name)
    {
        base.Rename(data, name);
        foreach (var sceneManagerData in data.Data.SceneManagers)
        {
            foreach (var transition in sceneManagerData.Transitions.ToArray())
            {
                if (transition.CommandIdentifier == Identifier)
                { 
                    transition.Name = name;
                }
            }
        }
    }
}