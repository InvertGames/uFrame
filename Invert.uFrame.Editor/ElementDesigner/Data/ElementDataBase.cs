using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ElementDataBase : DiagramItem, ISubSystemType
{
    public static Dictionary<string, string> TypeNameAliases = new Dictionary<string, string>()
    {
        {"System.Int32","int"},
        {"System.Boolean","bool"},
        {"System.Char","char"},
        {"System.Decimal","decimal"},
        {"System.Double","double"},
        {"System.Single","float"},
        {"System.String","string"},
    };

    public ElementDataBase BaseElement { get { return Data.AllElements.FirstOrDefault(p => p.Name == BaseTypeShortName); } }

    public abstract string BaseTypeName { get; set; }

    public string BaseTypeShortName
    {
        get
        {
            if (string.IsNullOrEmpty(BaseTypeName))
            {
                return UFrameAssetManager.DesignerVMAssemblyName.Split(',').FirstOrDefault();
            }
            return BaseTypeName.Split(',').FirstOrDefault();
        }
    }

    public abstract ICollection<ViewModelCollectionData> Collections { get; set; }

    public abstract ICollection<ViewModelCommandData> Commands { get; set; }

    public Type ControllerBaseType
    {
        get
        {
            if (IsControllerDerived)
            {
                return Type.GetType(UFrameAssetManager.DesignerVMAssemblyName.Replace("ViewModel", BaseTypeShortName.Replace("ViewModel", "") + "ControllerBase"));
            }
            return Type.GetType(UFrameAssetManager.DesignerVMAssemblyName.Replace("ViewModel", Name.Replace("ViewModel", "") + "ControllerBase"));
        }
    }

    public string ControllerName
    {
        get { return string.Format("{0}Controller", Name.Replace("ViewModel", "")); }
    }

    public string NameAsControllerBase
    {
        get
        {
            //if (IsControllerDerived)
            //{
            //    return string.Format("{0}Controller", BaseTypeShortName.Replace("ViewModel", ""));
            //}
            return string.Format("{0}ControllerBase", Name.Replace("ViewModel", ""));
        }
    }

    public Type ControllerType
    {
        get { return Type.GetType(UFrameAssetManager.DesignerVMAssemblyName.Replace("ViewModel", Name.Replace("ViewModel", "") + "Controller")); }
    }

    public Type CurrentViewModelType
    {
        get
        {
            var name = UFrameAssetManager.DesignerVMAssemblyName.Replace("ViewModel", Name.Replace("ViewModel", "") + "ViewModel");
            return Type.GetType(name);
        }
    }

    public bool IsControllerDerived
    {
        get { return !string.IsNullOrEmpty(BaseTypeName) && BaseTypeShortName != "ViewModel"; }
    }

    [SerializeField]
    private bool _isMultiInstance;

    [DiagramContextMenu("Has Multiple Instances",2)]
    public bool IsMultiInstance
    {
        get
        {
            return IsForcedMultiInstance || _isMultiInstance;
        }
        set
        {
            if (IsForcedMultiInstance && !value)
            {
                throw new Exception("This element belongs to a collection so it can NOT be a single instance element.");
            }
            _isMultiInstance = value;
        }
    }

    public bool IsForcedMultiInstance
    {
        get
        {
            return
                Data.AllDiagramItems.OfType<ElementDataBase>()
                    .SelectMany(p => p.Collections)
                    .Any(p => p.RelatedTypeName == Name) || AllBaseTypes.Any(p => p.IsMultiInstance);
        }
    }
    
    //[DiagramContextMenu("Print Items")]
    //public void Print()
    //{
    //    Debug.Log(BaseTypeName + ": " +string.Join(Environment.NewLine, AllBaseTypes.Select(p => p.Name).ToArray()));
    //}
    public IEnumerable<ElementDataBase> AllBaseTypes
    {
        get
        {
            var baseType = BaseElement;
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseElement;
            }
        }
    }

    public override IEnumerable<IDiagramSubItem> Items
    {
        get
        {
            return Properties.Cast<IDiagramSubItem>()
                .Concat(Collections.Cast<IDiagramSubItem>())
                .Concat(Commands.Cast<IDiagramSubItem>());
        }
    }

    public override string Label
    {
        get
        {
            return Name + (IsMultiInstance ? "*" : "");
        }
    }

    public string NameAsController
    {
        get { return string.Format("{0}Controller", Name); }
    }

    public string NameAsVariable
    {
        get { return char.ToLower(Name.First()) + Name.Substring(1); }
    }

    public string NameAsView
    {
        get
        {
            return string.Format("{0}View", Name);
        }
    }

    public string NameAsViewBase
    {
        get
        {
            return string.Format("{0}ViewBase", Name);
        }
    }

    public string NameAsViewModel
    {
        get { return string.Format("{0}ViewModel", Name.Replace("ViewModel", "")); }
    }

    public abstract ICollection<ViewModelPropertyData> Properties { get; set; }

    public IEnumerable<IViewModelItem> ViewModelItems
    {
        get
        {
            return Properties.Cast<IViewModelItem>()
                .Concat(Collections.Cast<IViewModelItem>())
                .Concat(Commands.Cast<IViewModelItem>());
        }
    }

    public IEnumerable<ElementDataBase> DerivedElements
    {
        get
        {
            var derived = Data.AllElements.Where(p => p.BaseTypeShortName == Name);
            foreach (var derivedItem in derived)
            {
                yield return derivedItem;
                foreach (var another in derivedItem.DerivedElements)
                {
                    yield return another;
                }
            }
        }
    }

    public string NameAsTypeEnum
    {
        get { return string.Format("{0}Types", Name); }
    }

    public ElementDataBase RootElement
    {
        get { return AllBaseTypes.LastOrDefault(); }
    }

    public override bool CanCreateLink(IDrawable target)
    {
        if (target is ViewData) return true;
        
        var elementData = target as ElementDataBase;
        return elementData != null && Name != elementData.Name && BaseTypeShortName != elementData.Name;
    }

    public override void CreateLink(IDiagramItem container, IDrawable target)
    {
        var element = target as ElementDataBase;
        if (element != null)
        {
            element.BaseTypeName = AssemblyQualifiedName;
        }
        var view = target as ViewData;
        if (view != null)
        {
            view.ForAssemblyQualifiedName = AssemblyQualifiedName;
        }
    }

    public override IEnumerable<IDiagramLink> GetLinks(IDiagramItem[] elementDesignerData)
    {
        foreach (var modelData in elementDesignerData.OfType<ElementDataBase>())
        {
            if (BaseTypeShortName == modelData.Name)
            {
                yield return new InheritanceLink()
                {
                    Base = this,
                    Derived = modelData
                };
            }
        }
    }

    public override void BeginEditing()
    {

        base.BeginEditing();
        OldAssemblyName = AssemblyQualifiedName;
    }

    public string OldAssemblyName { get; set; }

    public override void EndEditing()
    {
        base.EndEditing();
        var newText = Name;

        if (Data.ViewModels.Count(p => p.Name == newText || p.Name == OldName) > 1)
        {
            return;
        }
        foreach (var item in Data.ViewModels.Where(p => p.BaseTypeShortName == OldName))
        {
            item.BaseTypeName = AssemblyQualifiedName;
        }
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
        foreach (var result in Data.Views.Where(p => p.ForAssemblyQualifiedName == OldAssemblyName))
        {
            result.ForAssemblyQualifiedName = AssemblyQualifiedName;
        }
    }

    public virtual string ViewModelAssemblyQualifiedName
    {
        get
        {
            return UFrameAssetManager.DesignerVMAssemblyName.Replace("ViewModel", NameAsViewModel);
        }
    }
    //[DiagramContextMenu("")]
    //public void CreateNewBehaviour()
    //{
        
    //}
}