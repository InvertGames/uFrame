using System;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor.Refactoring;
using UnityEngine;
using UnityEditor;

[Serializable]
public class ViewComponentData : DiagramItem
{
  
    [SerializeField]
    private string _elementIdentifier;
    [SerializeField]
    private string _baseIdentifier;

    public override string Label
    {
        get { return Name; }
    }

    public string ElementIdentifier
    {
        get { return _elementIdentifier; }
        set { _elementIdentifier = value; }
    }

    public string BaseIdentifier
    {
        get { return _baseIdentifier; }
        set { _baseIdentifier = value;
            Dirty = true;
        }
    }

    public ViewComponentData Base
    {
        get { return Data.ViewComponents.FirstOrDefault(p => p.Identifier == this.BaseIdentifier); }
    }

    public ElementData Element
    {
        get
        {
            if (Base != null)
            {
                return Base.Element;
            }
            return Data.ViewModels.FirstOrDefault(p => p.Identifier == ElementIdentifier);
        }
    }
    public override RenameRefactorer CreateRenameRefactorer()
    {
        return new RenameViewComponentRefactorer(this);
    }

    public IEnumerable<ViewComponentData> AllBaseTypes
    {
        get
        {
            var t = this.Base;
            while (t != null)
            {
                yield return t;
                t = t.Base;
            }
        }
    }
    public override void CreateLink(IDiagramItem container, IDrawable target)
    {
        var viewComponentData = target as ViewComponentData;
        if (viewComponentData != null)
        {
            viewComponentData.BaseIdentifier = this.Identifier;
            viewComponentData.ElementIdentifier = null;
        }
        var element = target as ElementData;
        if (element != null)
            ElementIdentifier = element.Identifier;
    }

    public override bool CanCreateLink(IDrawable target)
    {
        var viewComponent = target as ViewComponentData;
        if (viewComponent != null)
        {
            if (Element == null)
            {
                return false;
            }
                
            if (viewComponent.Identifier == this.BaseIdentifier)
            {
                return false;
            }
            if (AllBaseTypes.Any(p => p.Identifier == viewComponent.Identifier))
            {
                return false;
            }
        }
        var elementData = target as ElementData;
        if (elementData != null)
        {
            if (Base != null && Base.Element != null)
            {
                return false;
            }
        }
        return target is ViewComponentData || target is ElementData;
    }

    public override IEnumerable<IDiagramLink> GetLinks(IDiagramItem[] elementDesignerData)
    {

        foreach (var diagramItem in elementDesignerData.OfType<ViewComponentData>().Where(p=>p.BaseIdentifier == Identifier))
        {
            yield return new ViewComponentLink()
            {
                Base = this,
                Derived = diagramItem
            };
        }

        var element = elementDesignerData.FirstOrDefault(p => p.Identifier == ElementIdentifier);
        if (element != null)
        {
            if (element != this && element.Identifier != this.Identifier)
            {
                yield return new GenericLink()
                {
                    Element = element,
                    Item = this
                };
            }
        }
    }

    public override void RemoveLink(IDiagramItem target)
    {
        var viewComponent = target as ViewComponentData;
        if (viewComponent != null)
        {
            viewComponent.BaseIdentifier = null;
            viewComponent.Dirty = true;
        }
    }

    public override IEnumerable<IDiagramSubItem> Items
    {
        get { yield break; }
    }

    public Type CurrentType
    {
        get { return Type.GetType(AssemblyQualifiedName); }
    }

    public override void EndEditing()
    {
        base.EndEditing();
    }

    public override void RemoveFromDiagram()
    {
        Data.ViewComponents.Remove(this);
        foreach (var viewComponentData in Data.ViewComponents)
        {
            if (viewComponentData.BaseIdentifier == this.Identifier)
            {
                viewComponentData.BaseIdentifier = null;
            }
        }
    }

    [DiagramContextMenu("Open Code")]
    public void OpenViewComponent(IElementsDataRepository repository)
    {
        var gameObject = new GameObject();
        var behaviour = gameObject.AddComponent(this.Name) as MonoBehaviour;
        if (behaviour != null)
        {
            var monoScript = MonoScript.FromMonoBehaviour(behaviour);
            AssetDatabase.OpenAsset(monoScript);
        }
        GameObject.DestroyImmediate(gameObject);
    }
}