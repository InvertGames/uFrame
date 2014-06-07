using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Invert.uFrame.Editor.ElementDesigner.Data;
using Invert.uFrame.Editor.Refactoring;
using UnityEngine;

[SerializeField]
public class ElementDesignerData : ScriptableObject, IRefactorable
{
    [SerializeField]
    private Color _associationLinkColor = Color.white;
    [SerializeField]
    private Color _definitionLinkColor = Color.cyan;

    [SerializeField]
    private Color _transitionLinkColor = Color.green;

    [SerializeField]
    private Color _subSystemLinkColor = Color.grey;

    [SerializeField]
    private Color _inheritanceLinkColor = Color.red;

    [SerializeField]
    private Color _SceneManagerLinkColor = Color.yellow;

    [SerializeField]
    private Color _viewLinkColor = Color.magenta;

    [SerializeField, HideInInspector]
    private List<EnumData> _enums = new List<EnumData>();

    private Stack<IDiagramFilter> _filterStack;

    [SerializeField, HideInInspector]
    private DefaultFilter _defaultFilter = new DefaultFilter();

   

    [SerializeField, HideInInspector]
    private List<ImportedElementData> _importedElements = new List<ImportedElementData>();
    private List<IDiagramLink> _links = new List<IDiagramLink>();

    [SerializeField, HideInInspector]
    private List<string> _persistedFilterStack = new List<string>();

    [SerializeField, HideInInspector]
    private SceneFlowFilter _sceneFlowFilter = new SceneFlowFilter();
    [SerializeField, HideInInspector]
    private List<SceneManagerData> _SceneManagers = new List<SceneManagerData>();

    [SerializeField]
    private float _snapSize = 10f;
    [SerializeField, HideInInspector]
    private List<SubSystemData> _subSystems = new List<SubSystemData>();
    [SerializeField, HideInInspector]
    private string _version;

    [SerializeField, HideInInspector]
    private List<ViewComponentData> _viewComponents = new List<ViewComponentData>();
    [SerializeField, HideInInspector]//, HideInInspector]
    private List<ElementData> _viewModels = new List<ElementData>();

    [SerializeField, HideInInspector]
    private List<ViewData> _views = new List<ViewData>();

    [SerializeField, HideInInspector]
    private List<PluginData> _pluginItems = new List<PluginData>();
    [SerializeField, HideInInspector]
    private string _codePathStrategyName = "Default";

    private static DiagramPlugin[] _plugins;

    public string ControllersFileName
    {
        get
        {
            return name + "Controllers.designer.cs";
        }
    }
    public string ViewModelsFileName
    {
        get
        {
            return name + ".designer.cs";
        }
    }
    public string ViewsFileName
    {
        get
        {
            return name + "Views.designer.cs";
        }
    }

    public IEnumerable<IDiagramItem> AllDiagramItems
    {
        get
        {
            return
                ViewModels.Cast<IDiagramItem>()
                    .Concat(ImportedElements.Cast<IDiagramItem>())
                    .Concat(Enums.Cast<IDiagramItem>())
                    .Concat(Views.Cast<IDiagramItem>())
                    .Concat(SceneManagers.Cast<IDiagramItem>())
                    .Concat(SubSystems.Cast<IDiagramItem>())
                    .Concat(ViewComponents.Cast<IDiagramItem>()
                );
        }
    }

    public IEnumerable<ElementDataBase> AllElements
    {
        get { return AllDiagramItems.OfType<ElementDataBase>(); }
    }

    public IEnumerable<IDiagramItem> AllowedDiagramItems
    {
        get { return AllDiagramItems.Where(p => CurrentFilter.IsAllowed(p, p.GetType())); }
    }

    public Color AssociationLinkColor
    {
        get { return _associationLinkColor; }
        set { _associationLinkColor = value; }
    }

    public IDiagramFilter CurrentFilter
    {
        get
        {
            if (FilterStack.Count < 1)
            {
                return SceneFlowFilter;
            }
            return FilterStack.Peek();
        }
    }

    public DefaultFilter DefaultFilter
    {
        get { return _defaultFilter; }
        set { _defaultFilter = value; }
    }

    public Color DefinitionLinkColor
    {
        get { return _definitionLinkColor; }
        set { _definitionLinkColor = value; }
    }

    public IEnumerable<IDiagramItem> DiagramItems
    {
        get { return FilterItems(AllDiagramItems); }
    }

    public IEnumerable<ElementDataBase> Elements
    {
        get { return DiagramItems.OfType<ElementDataBase>(); }
    }

    public List<EnumData> Enums
    {
        get { return _enums; }
        set { _enums = value; }
    }

    public IEnumerable<IDiagramFilter> FilterPath
    {
        get { return FilterStack.Reverse(); }
    }

    //public bool GenerateSceneManager
    //{
    //    get { return _generateSceneManager; }
    //    set { _generateSceneManager = value; }
    //}

    public IEnumerable<IDiagramFilter> Filters
    {
        get { return AllDiagramItems.OfType<IDiagramFilter>(); }
    }



    public IEnumerable<IDiagramItem> ImportableItems
    {
        get
        {
            //return AllowedDiagramItems;
            return AllowedDiagramItems.Where(p => !CurrentFilter.Locations.Keys.Contains(p.Identifier)).ToArray();
            //return items.Where(p => !Filters.Any(x => x.Locations.Keys.Contains(p.Identifier)));
        }
    }

    public List<ImportedElementData> ImportedElements
    {
        get { return _importedElements; }
        set { _importedElements = value; }
    }

    public Color InheritanceLinkColor
    {
        get { return _inheritanceLinkColor; }
        set { _inheritanceLinkColor = value; }
    }

    public List<IDiagramLink> Links
    {
        get { return _links; }
        set { _links = value; }
    }

    public string Name
    {
        get
        {
            return Regex.Replace(name, "[^a-zA-Z0-9_.]+", "");
        }
    }

    public SceneFlowFilter SceneFlowFilter
    {
        get
        {
            return _sceneFlowFilter;
        }
        set { _sceneFlowFilter = value; }
    }

    public Color SceneManagerLinkColor
    {
        get { return _SceneManagerLinkColor; }
        set { _SceneManagerLinkColor = value; }
    }

    public List<SceneManagerData> SceneManagers
    {
        get { return _SceneManagers; }
        set { _SceneManagers = value; }
    }

    public float SnapSize
    {
        get { return _snapSize; }
        set { _snapSize = value; }
    }

    public Color SubSystemLinkColor
    {
        get { return _subSystemLinkColor; }
        set { _subSystemLinkColor = value; }
    }

    public List<SubSystemData> SubSystems
    {
        get { return _subSystems; }
        set { _subSystems = value; }
    }

    public Color TransitionLinkColor
    {
        get { return _transitionLinkColor; }
        set { _transitionLinkColor = value; }
    }

    public string Version
    {
        get { return _version; }
        set { _version = value; }
    }

    public List<ViewComponentData> ViewComponents
    {
        get { return _viewComponents; }
        set { _viewComponents = value; }
    }

    public Color ViewLinkColor
    {
        get { return _viewLinkColor; }
        set { _viewLinkColor = value; }
    }

    public List<ElementData> ViewModels
    {
        get { return _viewModels; }
        set { _viewModels = value; }
    }

    public List<ViewData> Views
    {
        get { return _views; }
        set { _views = value; }
    }

    protected Stack<IDiagramFilter> FilterStack
    {
        get
        {
            return _filterStack ?? (_filterStack = new Stack<IDiagramFilter>());
        }
        set { _filterStack = value; }
    }

    public void ApplyFilter()
    {
        UpdateLinks();
        //foreach (var item in DiagramItems)
        //{
        //    item.Filter = CurrentFilter;
        //}
        //UpdateLinks();
    }

    public void CleanUpFilters()
    {
        var diagramItems = AllDiagramItems.Select(p => p.Identifier);

        foreach (var diagramFilter in Filters)
        {
            var removeKeys = diagramFilter.Locations.Keys.Where(p => !diagramItems.Contains(p)).ToArray();
            foreach (var removeKey in removeKeys)
            {
                diagramFilter.Locations.Remove(removeKey);
            }
        }
        //UpdateLinks();
    }

    public IEnumerable<IDiagramItem> FilterItems(IDiagramFilter filter)
    {
        return FilterItems(filter, AllDiagramItems);
    }

    public IEnumerable<IViewModelItem> GetAllBaseItems(ElementDataBase data)
    {
        var current = data;
        while (current != null)
        {
            foreach (var item in current.Items)
            {
                if (item is IViewModelItem)
                {
                    yield return item as IViewModelItem;
                }
            }

            current = AllElements.FirstOrDefault(p => p.AssemblyQualifiedName == current.BaseTypeName);
        }
    }

    public string GetUniqueName(string name)
    {
        var tempName = name;
        var index = 1;
        while (AllDiagramItems.Any(p => p.Name.ToUpper() == tempName.ToUpper()))
        {
            tempName = name + index;
            index++;
        }
        return tempName;
    }

    public ElementDataBase[] GetAssociatedElements(ElementDataBase data)
    {
        return GetAssociatedElementsInternal(data).Concat(new[] { data }).Distinct().ToArray();
    }

    public ElementDataBase GetElement(IViewModelItem item)
    {
        if (item.RelatedTypeName == null)
        {
            return null;
        }
        return AllElements.FirstOrDefault(p => p.Name == item.RelatedTypeName);
    }

    public IEnumerable<IDiagramFilter> GetFilters(IDiagramFilter filter)
    {
        //yield return DefaultFilter;
        //yield return SceneFlowFilter;

        foreach (var allDiagramItem in FilterItems(filter).OfType<IDiagramFilter>())
        {
            yield return allDiagramItem;
        }
    }

    public ElementData GetViewModel(string elementName)
    {
        return ViewModels.FirstOrDefault(p => p.Name == elementName);
    }

    public void PopFilter()
    {
        FilterLeave();
        _persistedFilterStack.Remove(FilterStack.Peek().Name);
        FilterStack.Pop();
        ApplyFilter();
    }

    public void PopToFilter(IDiagramFilter filter1)
    {
        while (CurrentFilter != filter1)
        {
            PopFilter();
        }
    }
    public void PopToFilter(string filterName)
    {
        while (CurrentFilter.Name != filterName)
        {
            PopFilter();
        }
    }
    public void PushFilter(IDiagramFilter filter)
    {
        FilterLeave();
        FilterStack.Push(filter);
        if (!_persistedFilterStack.Contains(filter.Name))
            _persistedFilterStack.Add(filter.Name);
        ApplyFilter();
    }

    public void ReloadFilterStack()
    {
        if (_persistedFilterStack.Count != (FilterStack.Count))
        {
            foreach (var filterName in _persistedFilterStack)
            {
                var filter = Filters.FirstOrDefault(p => p.Name == filterName);
                if (filter == null)
                {
                    _persistedFilterStack.Clear();
                    FilterStack.Clear();
                    break;
                }
                PushFilter(filter);
            }
        }
    }

    public void UpdateLinks()
    {
        CleanUpFilters();
        Links.Clear();

        var items = DiagramItems.SelectMany(p => p.Items).Where(p => CurrentFilter.IsItemAllowed(p, p.GetType())).ToArray();
        var diagramItems = this.DiagramItems.ToArray();
        foreach (var item in items)
        {
            Links.AddRange(item.GetLinks(diagramItems));
        }

        var diagramFilter = CurrentFilter as IDiagramItem;
        if (diagramFilter != null)
        {
            var diagramFilterItems = diagramFilter.Items.OfType<IDiagramItem>().ToArray();
            foreach (var diagramItem in diagramItems)
            {
                Links.AddRange(diagramItem.GetLinks(diagramFilterItems));
            }
            //foreach (var diagramFilterItem in diagramFilterItems)
            //{
            //    Links.AddRange(diagramFilterItem.GetLinks(diagramItems));
            //}
        }

        var models = DiagramItems.ToArray();

        foreach (var viewModelData in models)
        {
            //viewModelData.Filter = CurrentFilter;
            Links.AddRange(viewModelData.GetLinks(diagramItems));
        }
    }



    protected IEnumerable<IDiagramItem> FilterItems(IEnumerable<IDiagramItem> allDiagramItems)
    {
        return FilterItems(CurrentFilter, allDiagramItems);
    }

    protected IEnumerable<IDiagramItem> FilterItems(IDiagramFilter filter, IEnumerable<IDiagramItem> allDiagramItems)
    {
        foreach (var item in allDiagramItems)
        {
            if (filter.IsAllowed(item, item.GetType()))
            {
                if (filter.ImportedOnly && filter != item)
                {
                    if (filter.Locations.Keys.Contains(item.Identifier))
                    {
                        yield return item;
                    }
                }
                else
                {
                    yield return item;
                }
            }
        }
    }

    protected void FilterLeave()
    {
    }

    protected IEnumerable<ElementDataBase> GetAssociatedElementsInternal(ElementDataBase data)
    {
        var derived = GetAllBaseItems(data);
        foreach (var viewModelItem in derived)
        {
            var element = GetElement(viewModelItem);
            if (element != null)
            {
                yield return element;
                var subItems = GetAssociatedElementsInternal(element);
                foreach (var elementDataBase in subItems)
                {
                    yield return elementDataBase;
                }
            }
        }
    }

    public List<Refactorer> Refactorings
    {
        get
        {
            return
                AllDiagramItems.OfType<IRefactorable>()
                    .SelectMany(p => p.Refactorings)
                    .Concat(AllDiagramItems.SelectMany(p => p.Items).OfType<IRefactorable>().SelectMany(p => p.Refactorings))
                    .ToList();
        }
    }

    public List<PluginData> PluginItems
    {
        get { return _pluginItems; }
        set { _pluginItems = value; }
    }

    public string AssetPath { get; set; }

    public string CodePathStrategyName
    {
        get { return string.IsNullOrEmpty(_codePathStrategyName) ? "Default" : _codePathStrategyName; }
        set { _codePathStrategyName = value; }
    }

    public ICodePathStrategy CodePathStrategy { get; set; }

    public void RefactorApplied()
    {
        var refactorables = AllDiagramItems.OfType<IRefactorable>()
            .Concat(AllDiagramItems.SelectMany(p => p.Items).OfType<IRefactorable>());
        foreach (var refactorable in refactorables)
        {
            refactorable.RefactorApplied();
        }
    }
}