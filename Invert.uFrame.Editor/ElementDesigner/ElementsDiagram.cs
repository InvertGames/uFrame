using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ElementDesigner.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ElementsDiagram : ICommandHandler
{
    public delegate void SelectionChangedEventArgs(IDiagramItem oldData, IDiagramItem newData);

    public delegate void ViewModelDataEventHandler(ElementData data);

    public event ViewModelDataEventHandler AddNewBehaviourClicked;

    public event SelectionChangedEventArgs SelectionChanged;

    private static DiagramPlugin[] _plugins;

    private ElementDesignerData _data;

    private List<IElementDrawer> _elementDrawers = new List<IElementDrawer>();

    private IElementDrawer _selected;

    private ISelectable _selectedItem;

    private SerializedObject _serializedObject;
    private Event _currentEvent;
    private SerializedObject _o;

    public IEnumerable<IDiagramItem> AllSelected
    {
        get
        {
            return Data.DiagramItems.Where(p => p.IsSelected);
        }
    }

    public IDiagramItem CurrentMouseOverItem { get; set; }

    public ElementDesignerData Data
    {
        get { return _data; }
        set
        {
            _data = value;
            if (_data != null)
            {
                _data.ReloadFilterStack();
            }
            Refresh(true);
        }
    }

    public Rect DiagramSize
    {
        get
        {
            Rect size = new Rect();
            foreach (var diagramItem in this.Data.DiagramItems)
            {
                var rect = diagramItem.Position.Scale(Scale);

                if (rect.x < 0)
                    rect.x = 0;
                if (rect.y < 0)
                    rect.y = 0;
                //if (rect.x < size.x)
                //{
                //    size.x = rect.x;
                //}
                //if (rect.y < size.y)
                //{
                //    size.y = rect.y;
                //}
                if (rect.x + rect.width > size.x + size.width)
                {
                    size.width = rect.x + rect.width;
                }
                if (rect.y + rect.height > size.y + size.height)
                {
                    size.height = rect.y + rect.height;
                }
            }
            size.height += 400f;
            size.width += 400f;
            if (size.height < Screen.height)
            {
                size.height = Screen.height;
            }
            if (size.width < Screen.width)
            {
                size.width = Screen.width;
            }
            return size;
        }
    }

    public bool DidDrag { get; set; }

    public bool Dirty { get; set; }

    public Vector2 DragDelta
    {
        get
        {
            var mp = CurrentMousePosition;

            var v = new Vector2(Mathf.Round(mp.x / SnapSize) * SnapSize, Mathf.Round(mp.y / SnapSize) * SnapSize);
            var v2 = new Vector2(Mathf.Round(LastDragPosition.x / SnapSize) * SnapSize, Mathf.Round(LastDragPosition.y / SnapSize) * SnapSize);
            return (v - v2);
        }
    }

    public List<IElementDrawer> ElementDrawers
    {
        get { return _elementDrawers; }
        set { _elementDrawers = value; }
    }

    public bool IsMouseDown { get; set; }

    public Vector2 LastDragPosition { get; set; }

    public Vector2 LastMouseDownPosition { get; set; }

    public Vector2 LastMouseUpPosition { get; set; }

    public static float Scale
    {
        get { return UFStyles.Scale; }
    }

    public IElementDrawer MouseOverViewData
    {
        get
        {
            return ElementDrawers.FirstOrDefault(p => p.Model.Position.Scale(Scale).Contains(CurrentMousePosition));
            //return Data.DiagramItems.LastOrDefault(p => p.Position.Contains(CurrentMousePosition));
        }
    }

    protected IElementsDataRepository Repository { get; set; }

    public IDiagramItem SelectedData
    {
        get
        {
            if (Selected == null) return null;
            return Selected.Model;
        }
    }
    
    public IElementDrawer Selected
    {
        get { return _selected; }
        set
        {
            var old = _selected;
            _selected = value;
            // Data.ViewModels.ForEach(p => p.IsSelected = false);
            if (_selected != null)
            {
                if (!_selected.Model.IsCollapsed)
                    SelectedItem = _selected.Items.OfType<ISelectable>().FirstOrDefault(p => p.Position.Scale(Scale).Contains(CurrentMousePosition));

                _selected.Model.IsSelected = true;
            }
            else
            {
                SelectedItem = null;
            }

            if (old != value && value != null)
            {

                OnSelectionChanged(old == null ? null : old.Model, value.Model);
            }
        }
    }

    public ISelectable SelectedItem
    {
        get { return _selectedItem; }
        set
        {
            foreach (var item in ElementDrawers.SelectMany(p => p.Items))
                item.IsSelected = false;

            var old = _selectedItem;
            //Data.DiagramItems.ToList().ForEach(p => p.IsSelected = false);
            GUI.FocusControl("");

            _selectedItem = value;
            if (old != value)
            {
                OnSelectionChanged(SelectedData, SelectedData);
            }
            if (_selectedItem != null)
            {
                _selectedItem.IsSelected = true;
            }
        }
    }

    public Vector2 SelectionOffset { get; set; }

    public Rect SelectionRect { get; set; }

    private SerializedObject SerializedObject
    {
        get { return _o ?? (_o = new SerializedObject(Data)); }
        set { _o = value; }
    }

    public ElementsDiagram(string assetPath)
    {
        var fileExtension = Path.GetExtension(assetPath);
        Repository = uFrameEditor.Container.Resolve<IElementsDataRepository>(fileExtension);
        if (Repository != null)
        {
            Repository.LoadDiagram(assetPath);
        }
        else
        {
            throw new Exception(
                string.Format("The asset with the extension {0} could not be loaded.  Do you have the plugin installed?",
                fileExtension
                ));
        }

        Data = Repository.LoadDiagram(assetPath);
        

        CodePathStrategy = uFrameEditor.Container.Resolve<ICodePathStrategy>(Data.CodePathStrategyName ?? "Default");
        CodePathStrategy.AssetPath = assetPath.Replace(string.Format("{0}{1}", Path.GetFileNameWithoutExtension(assetPath), fileExtension), ""); 
    }

    

    public ICodePathStrategy CodePathStrategy { get; set; }

    public void ExecuteCommand(IEditorCommand command,object arg)
    {
        Undo.RecordObject(Data, command.Title);
        command.Execute(arg);
        Refresh(true);
        EditorUtility.SetDirty(Data);
    }

    public IElementDrawer CreateDrawerFor(IDiagramItem item)
    {
        return uFrameEditor.CreateDrawer(item, this);
        //foreach (var diagramPlugin in Plugins)
        //{
        //    var drawer = diagramPlugin.GetDrawer(this, item);
        //    if (drawer != null)
        //    {
        //        return drawer;
        //    }
        //}
        //var type = item.GetType();
        //var elementData = item as ElementDataBase;
        //if (elementData != null)
        //{
        //    var drawer = new ElementDrawer(elementData, this);

        //    var data = elementData as ElementData;
        //    if (data != null)
        //    {
        //        drawer.PropertiesHeader.OnAddItem += () => AddNewProperty(data);
        //        drawer.CollectionsHeader.OnAddItem += () => AddNewCollection(data);
        //        drawer.CommandsHeader.OnAddItem += () => AddNewCommand(data);
        //        drawer.BehavioursHeader.OnAddItem += () => AddNewBehaviour(data);
        //    }
        //    return drawer;
        //}
        //if (type == typeof(EnumData))
        //{
        //    return new DiagramEnumDrawer(item as EnumData, this);
        //}
        //if (type == typeof(ViewComponentData))
        //{
        //    return new ViewComponentDrawer(item as ViewComponentData, this);
        //}
        //if (type == typeof(SubSystemData))
        //{
        //    return new SubSystemDrawer(item as SubSystemData, this);
        //}
        //if (type == typeof(ViewData))
        //{
        //    return new ViewDrawer(item as ViewData, this);
        //}
        //if (type == typeof(SceneManagerData))
        //{
        //    return new SceneManagerDrawer(item as SceneManagerData, this);
        //}
        //if (type == typeof(EnumData))
        //{
        //    return new DiagramEnumDrawer(item as EnumData, this);
        //}
        //if (type == typeof(PluginData))
        //{
        //    var data = item as PluginData;
        //    if (data == null) return null;
        //    foreach (var diagramPlugin in Plugins)
        //    {
        //        if (diagramPlugin.GetType() == data.PluginType)
        //        {
        //            return diagramPlugin.GetDrawer(this, data);
        //        }
        //    }
        //}
        //return null;
    }

    public void DeselectAll()
    {
        foreach (var diagramItem in AllSelected)
        {
            if (diagramItem.IsEditing)
            {
                diagramItem.EndEditing();
            }
            diagramItem.IsSelected = false;
        }
    }

    public Vector2 CurrentMousePosition
    {
        get { return CurrentEvent.mousePosition; }
    }

    public float SnapSize
    {
        get { return Data.SnapSize * Scale; }
    }

    public void Save()
    {
        Repository.SaveDiagram(Data);
    }

    public void Draw()
    {
        //var beforeScale = GUI.matrix;
        //var guiTranslation = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, Vector3.one);
        //var guiScale = Matrix4x4.Scale(new Vector3(0.75f, 0.75f, 0.75f));
        //GUI.matrix = guiTranslation * guiScale * guiTranslation.inverse;

        //Repository.FastUpdate();
        SerializedObject.Update();
        string focusItem = null;

        foreach (var drawer in ElementDrawers.OrderBy(p => p.IsSelected).ToArray())
        {
            //drawer.CalculateBounds();
            var shouldFocus = drawer.ShouldFocus;
            if (shouldFocus != null)
                focusItem = shouldFocus;
            drawer.Draw(this);
        }
        foreach (var link in Data.Links.ToArray())
        {
            link.Draw(this);
            link.DrawPoints(this);
        }
        //foreach (var link in Data.Links.ToArray())
        //{
        //}
        if (focusItem != null)
        {
            EditorGUI.FocusTextInControl(focusItem);
        }
        SerializedObject.ApplyModifiedProperties();
        //Repository.FastSave();
        if (!EditorApplication.isCompiling)
        {
            HandleInput();
        }

        if (IsMouseDown && SelectedData != null && SelectedItem == null && !CurrentEvent.control)
        {
            if (!DidDrag)
            {
                Undo.RecordObject(Data, "Move " + SelectedData.Name);
            }

            var newPosition = DragDelta;//CurrentMousePosition - SelectionOffset;
            foreach (var diagramItem in AllSelected)
            {
                diagramItem.Location += (newPosition * (1f / Scale));

                //diagramItem.Location = new Vector2(Mathf.Round((diagramItem.Location.x)/ SnapSize) * SnapSize, Mathf.Round(diagramItem.Location.y / SnapSize) * SnapSize);

                //var newPositionRect = new Rect(newPosition.x, newPosition.y, diagramItem.Position.width,
                //    diagramItem.Position.height);
                ////Math.Round(value / 5.0) * 5
                //diagramItem.Position = newPositionRect;
            }

            foreach (var viewModelDrawer in ElementDrawers)
            {
                viewModelDrawer.CalculateBounds();
            }
            DidDrag = true;
            LastDragPosition = CurrentMousePosition;
        }
        else if (IsMouseDown && SelectedData != null && CurrentEvent.control)
        {
            if (!SelectedData.Position.Scale(Scale).Contains(CurrentMousePosition))
            {
                if (SelectedItem == null)
                {
                    var mouseOver = MouseOverViewData;
                    var canCreateLink = SelectedData.CanCreateLink(mouseOver == null ? null : mouseOver.Model);

                    CurrentMouseOverItem = mouseOver == null ? null : mouseOver.Model;

                    UFStyles.DrawNodeCurve(SelectedData.Position.Scale(UFStyles.Scale), mouseOver != null && canCreateLink ? CurrentMouseOverItem.Position.Scale(UFStyles.Scale) :
                            new Rect(CurrentMousePosition.x, CurrentMousePosition.y, 4, 4), Color.yellow, 6);
                }
                else
                {
                    try
                    {
                        var mouseOver = MouseOverViewData;
                        var canCreateLink = SelectedItem.CanCreateLink(mouseOver == null ? null : mouseOver.Model);

                        CurrentMouseOverItem = mouseOver == null ? null : mouseOver.Model;

                        UFStyles.DrawNodeCurve(SelectedItem.Position.Scale(UFStyles.Scale),
                            CurrentMouseOverItem != null && canCreateLink
                                ? CurrentMouseOverItem.Position.Scale(UFStyles.Scale)
                                : new Rect(CurrentMousePosition.x, CurrentMousePosition.y, 4, 4),
                            Color.green, 6);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.Message);
                    }

                }
            }
            else
            {
                CurrentMouseOverItem = null;
            }
        }
        else if (IsMouseDown)
        {
            var cur = CurrentMousePosition;
            if (cur.x > LastMouseDownPosition.x)
            {
                if (cur.y > LastMouseDownPosition.y)
                {
                    SelectionRect = new Rect(LastMouseDownPosition.x, LastMouseDownPosition.y,
                        cur.x - LastMouseDownPosition.x, cur.y - LastMouseDownPosition.y);
                }
                else
                {
                    SelectionRect = new Rect(
                        LastMouseDownPosition.x, cur.y, cur.x - LastMouseDownPosition.x, LastMouseDownPosition.y - cur.y);
                    //SelectionRect = new Rect(LastMouseDownPosition.x, LastMouseDownPosition.y,
                    //    cur.x - LastMouseDownPosition.x, cur.y - LastMouseDownPosition.y);
                }
            }
            else
            {
                if (cur.y > LastMouseDownPosition.y)
                {
                    // x is less and y is greater
                    SelectionRect = new Rect(
                        cur.x, LastMouseDownPosition.y, LastMouseDownPosition.x - cur.x, cur.y - LastMouseDownPosition.y);
                }
                else
                {
                    // both are less
                    SelectionRect = new Rect(
                    cur.x, cur.y, LastMouseDownPosition.x - cur.x, LastMouseDownPosition.y - cur.y);
                }
                //SelectionRect = new Rect(LastMouseDownPosition.x, LastMouseDownPosition.y,
                //   LastMouseDownPosition.x - cur.x, LastMouseDownPosition.y - cur.y);
            }
        }
        else
        {
            SelectionRect = new Rect();
            CurrentMouseOverItem = null;
        }
        if (SelectionRect.width > 20 && SelectionRect.height > 20)
        {
            foreach (var item in Data.DiagramItems)
            {
                item.IsSelected = SelectionRect.Overlaps(item.Position.Scale(Scale));
            }
            UFStyles.DrawExpandableBox(SelectionRect, UFStyles.BoxHighlighter4, string.Empty);
        }
    }

    public DiagramItemDrawer<TData> GetDrawer<TData>(TData data) where TData : IDiagramItem
    {
        return ElementDrawers.OfType<DiagramItemDrawer<TData>>().FirstOrDefault(p => p.Data.Equals(data));
    }

    public void HandleInput()
    {

        var e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            CurrentEvent = Event.current;
            LastMouseDownPosition = e.mousePosition;
            IsMouseDown = true;
            OnMouseDown();
            if (e.clickCount > 1)
            {
                OnDoubleClick();
            }
            e.Use();
        }
        if (CurrentEvent.rawType == EventType.MouseUp && IsMouseDown && e.button == 0)
        {
            LastMouseUpPosition = e.mousePosition;
            IsMouseDown = false;
            OnMouseUp();
            e.Use();
        }
        if (CurrentEvent.keyCode == KeyCode.Return)
        {
            if (SelectedData != null && SelectedData.IsEditing)
            {
                SelectedData.EndEditing();
                e.Use();
                this.Dirty = true;
            }
        }
        if (CurrentEvent.keyCode == KeyCode.F2)
        {
            if (SelectedData != null)
            {
                SelectedData.BeginEditing();
                e.Use();
            }
        }
    }

    public void Refresh(bool refreshDrawers = true)
    {
        if (refreshDrawers)
            ElementDrawers.Clear();

        foreach (var diagramItem in Data.DiagramItems)
        {
            diagramItem.Data = Data;
            diagramItem.Dirty = true;
            var drawer = CreateDrawerFor(diagramItem);
            if (drawer == null) continue;
            ElementDrawers.Add(drawer);

            if (refreshDrawers)
            {
                drawer.CalculateBounds();
            }
        }
        Data.UpdateLinks();
        Dirty = true;
    }

    public void ShowAddNewContextMenu(bool addAtMousePosition = false)
    {
        var menu = uFrameEditor.CreateCommandUI<ContextMenuUI>(this, typeof(IDiagramContextCommand));
        menu.Go();

        //var menu = new GenericMenu();
        //var contextCommands = uFrameEditor.GetContextCommandsFor<ElementsDiagram>();
        //foreach (var contextCommand in contextCommands)
        //{
        //    IEditorCommand command = contextCommand;
        //    menu.AddItem(new GUIContent(((IContextMenuItemCommand)contextCommand).Path), ((IContextMenuItemCommand)contextCommand).Checked, () =>
        //    {
        //        ExecuteCommand(command,this);
        //    });
        //}


        //if (Data.CurrentFilter.IsAllowed(null, typeof(ElementData)))
        //    menu.AddItem(new GUIContent("New Element"), false, () => { AddNewViewModel(addAtMousePosition); });

        //if (Data.CurrentFilter.IsAllowed(null, typeof(EnumData)))
        //    menu.AddItem(new GUIContent("New Enum"), false, () => { AddNewEnum(addAtMousePosition); });

        //if (Data.CurrentFilter.IsAllowed(null, typeof(ViewData)))
        //    menu.AddItem(new GUIContent("New View"), false, () => { AddNewView(addAtMousePosition); });

        //if (Data.CurrentFilter.IsAllowed(null, typeof(ViewComponentData)))
        //    menu.AddItem(new GUIContent("New View Component"), false, () => { AddNewViewComponent(addAtMousePosition); });

        //if (Data.CurrentFilter.IsAllowed(null, typeof(SceneManagerData)))
        //    menu.AddItem(new GUIContent("New Scene Manager"), false, () => { AddNewSceneManager(addAtMousePosition); });

        //if (Data.CurrentFilter.IsAllowed(null, typeof(SubSystemData)))
        //    menu.AddItem(new GUIContent("New Sub System"), false, () => { AddNewSubDiagram(addAtMousePosition); });

        //foreach (var diagramPlugin in Plugins)
        //{
        //    if (diagramPlugin == null) continue;
        //    diagramPlugin.OnAddContextItems(this, menu);
        //}

        //menu.AddSeparator("");
        //foreach (var item in Data.ImportableItems)
        //{
        //    IDiagramItem item1 = item;
        //    menu.AddItem(new GUIContent(item.Name), false, () =>
        //    {
        //        Undo.RecordObject(Data, "Import " + item1.Name);
        //        Data.CurrentFilter.Locations[item1] = new Vector2(0f, 0f);
        //        Refresh(true);
        //        EditorUtility.SetDirty(Data);
        //    });
        //}
        //menu.AddSeparator("");

        //menu.AddItem(new GUIContent("Import All"), false, () =>
        //{
        //    Undo.RecordObject(Data, "Import All");
        //    foreach (var importableItem in Data.ImportableItems)
        //    {
        //        Data.CurrentFilter.Locations[importableItem] = new Vector2(0f, 0f);
        //    }
        //    Refresh(true);
        //    EditorUtility.SetDirty(Data);
        //});

        //menu.ShowAsContext();
    }

    public void ShowContextMenu()
    {

        var menu = uFrameEditor.CreateCommandUI<ContextMenuUI>(Selected, typeof (IDiagramItemCommand), Selected.CommandsType);
        menu.Go();

        //var menu = new GenericMenu();

        //DecorateContextMenu(SelectedData, menu);

        //menu.AddSeparator(string.Empty);
        //var links = Data.Links.Where(p => p.Target == SelectedData);
        //foreach (var diagramLink in links)
        //{
        //    if (!(diagramLink.Target is IDiagramItem)) continue;

        //    IDiagramLink link = diagramLink;
        //    menu.AddItem(new GUIContent(diagramLink.Source.Label), true, () =>
        //    {
        //        link.Source.RemoveLink(link.Target as IDiagramItem);
        //        Refresh();
        //    });
        //}

        //menu.AddSeparator(string.Empty);
        //menu.AddItem(new GUIContent("Delete"), false, () =>
        //{
        //    var selected = SelectedData;
        //    var customFiles = Repository.GetCustomFilePaths(SelectedData, false).ToArray();
        //    var customFileFullPaths = Repository.GetCustomFilePaths(SelectedData, true).Where(File.Exists).ToArray();
        //    if (selected is IDiagramFilter)
        //    {
        //        var filter = selected as IDiagramFilter;
        //        if (filter.Locations.Keys.Count > 1)
        //        {
        //            EditorUtility.DisplayDialog("Delete sub items first.",
        //                "There are items defined inside this item please hide or delete them before removing this item.", "OK");
        //            return;
        //        }
        //    }
        //    if (EditorUtility.DisplayDialog("Confirm", "Are you sure you want to delete this?", "Yes", "No"))
        //    {

        //        selected.RemoveFromDiagram();
        //        if (customFileFullPaths.Length > 0)
        //        {
        //            if (EditorUtility.DisplayDialog("Confirm",
        //                "You have files associated with this. Delete them to?" + Environment.NewLine +
        //                string.Join(Environment.NewLine, customFiles), "Yes Delete Them", "Don't Delete them"))
        //            {
        //                foreach (var customFileFullPath in customFileFullPaths)
        //                {
        //                    File.Delete(customFileFullPath);
        //                }
        //                Repository.Save();
        //            }
        //        }

        //        Refresh(true);
        //    }
        //});

        //if (menu.GetItemCount() > 0)
        //{
        //    menu.ShowAsContext();
        //}
    }

    public void ShowItemContextMenu(object item)
    {
        var menu = new GenericMenu();
        DecorateContextMenu(item, menu);
        if (menu.GetItemCount() > 0)
        {
            menu.ShowAsContext();
        }
    }

    protected virtual void DecorateContextMenu(object context, GenericMenu menu)
    {
        //var methods = context.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
        //var properties = context.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        //foreach (var propertyInfo in properties)
        //{
        //    var attribute = propertyInfo.GetCustomAttributes(typeof(DiagramContextMenuAttribute), true).FirstOrDefault() as DiagramContextMenuAttribute;
        //    if (attribute == null) continue;
        //    var value = (bool)propertyInfo.GetValue(context, null);
        //    PropertyInfo info = propertyInfo;
        //    menu.AddItem(new GUIContent(attribute.MenuPath), value, () =>
        //    {
        //        try
        //        {
        //            info.SetValue(context, !value, null);
        //        }
        //        catch (Exception ex)
        //        {
        //            EditorUtility.DisplayDialog("Can't do that", ex.InnerException.Message, "OK");
        //        }
        //    });
        //}
        //if (menu.GetItemCount() > 0)
        //{
        //    menu.AddSeparator("");
        //}

        //var items = new List<ContextMenuItem>();
        //foreach (var methodInfo in methods)
        //{
        //    var attribute = methodInfo.GetCustomAttributes(typeof(DiagramContextMenuAttribute), true).FirstOrDefault() as DiagramContextMenuAttribute;
        //    if (attribute == null) continue;
        //    var item = new ContextMenuItem()
        //    {
        //        Attribute = attribute,
        //        ActionMethod = methodInfo,
        //    };
        //    items.Add(item);
        //}

        //foreach (var contextMenuItem in items.OrderBy(p => p.Attribute.Index))
        //{
        //    var checkedMethod = methods.FirstOrDefault(p => p.Name == contextMenuItem.ActionMethod.Name + "IsChecked");
        //    var check = false;
        //    if (checkedMethod != null)
        //    {
        //        check = (bool)checkedMethod.Invoke(context, null);
        //    }
        //    ContextMenuItem item = contextMenuItem;
        //    menu.AddItem(new GUIContent(contextMenuItem.Attribute.MenuPath), check, () =>
        //    {
        //        Undo.RecordObject(this.Data, item.ActionMethod.Name);
        //        var parameters = item.ActionMethod.GetParameters();
        //        if (parameters.Length > 0)
        //        {
        //            if (typeof(IDiagramItem).IsAssignableFrom(parameters[0].ParameterType))
        //            {
        //                item.ActionMethod.Invoke(context, new object[] { SelectedData });
        //            }
        //            else if (parameters[0].ParameterType == typeof(IElementsDataRepository))
        //            {
        //                item.ActionMethod.Invoke(context, new object[] { Repository });
        //            }
        //            else
        //            {
        //                item.ActionMethod.Invoke(context, new object[] { Data });
        //            }
        //        }
        //        else
        //        {
        //            item.ActionMethod.Invoke(context, null);
        //        }
        //        Refresh(true);
        //        EditorUtility.SetDirty(this.Data);
        //    });
        //}

        //foreach (var diagramPlugin in Plugins)
        //{
        //    if (diagramPlugin == null) continue;
        //    diagramPlugin.OnAddContextItems(this, menu);
        //}
    }

    protected virtual void OnSelectionChanged(IDiagramItem olddata, IDiagramItem newdata)
    {
        SelectionChangedEventArgs handler = SelectionChanged;
        if (handler != null) handler(olddata, newdata);
    }

    private void DoViewModelInspector(ElementDataBase selected)
    {
        // UBEditor.IsGlobals = true;
        // UBEditor.DoToolbar(Selected.Name + " Properties");
        EditorGUI.BeginChangeCheck();
        var text = EditorGUILayout.TextField("Name", selected.Name);
        if (EditorGUI.EndChangeCheck())
        {
            selected.Name = text;
            EditorUtility.SetDirty(Data);
        }

        //if (SelectedItem != null)
        //{
        //    UBEditor.DoToolbar(SelectedItem.Name + " Properties");
        //    var typesList = Repository.GetAvailableTypes();
        //    EditorGUI.BeginChangeCheck();
        //    var newName = EditorGUILayout.TextField(SelectedItem.Name);
        //    if (EditorGUI.EndChangeCheck())
        //    {
        //        SelectedItem.Name = newName;
        //        EditorUtility.SetDirty(Data);
        //    }
        //    if (GUILayout.Button(SelectedItem.RelatedType))
        //    {
        //        UBListWindow.Init("Choose Type", Data.name, typesList, (n, v) =>
        //        {
        //            SelectedItem.RelatedType = v;
        //            Data.UpdateLinks();
        //            EditorUtility.SetDirty(Data);
        //        });
        //    }

        //}
    }

    private void OnDoubleClick()
    {

        if (SelectedData != null)
        {
            if (SelectedItem == null)
            {
                if (SelectedData is IDiagramFilter)
                {
                    if (SelectedData == Data.CurrentFilter)
                    {
                        Data.PopFilter();
                    }
                    else
                    {
                        Data.PushFilter(SelectedData as IDiagramFilter);
                    }

                    Refresh(true);
                }
                else
                {
                    Selected.DoubleClicked();
                }
            }

        }

    }

    private void OnMouseDown()
    {
        var selected = MouseOverViewData;
        if (selected != null)
        {
            if (Selected != null)
            {
                if (CurrentEvent.shift)
                {
                }
                else
                {
                    if (AllSelected.All(p => p != selected.Model))
                    {
                        DeselectAll();
                    }
                }
            }
            SelectionOffset = LastMouseDownPosition - new Vector2(selected.Model.Position.x, selected.Model.Position.y);
            LastDragPosition = LastMouseDownPosition;
        }
        else
        {
            //if (AllSelected.All(p => p != SelectedData))
            //{
            foreach (var diagramItem in AllSelected)
            {
                if (diagramItem.IsEditing)
                {
                    diagramItem.EndEditing();
                }
                diagramItem.IsSelected = false;
            }
            //}
        }

        Selected = selected;
    }

    public Event CurrentEvent
    {
        get { return Event.current; }
        set { _currentEvent = value; }
    }

    private void OnMouseUp()
    {
        if (CurrentEvent.button == 1)
        {
            if (SelectedItem != null)
            {
                ShowItemContextMenu(SelectedItem);
            }
            else if (SelectedData != null)
            {
                ShowContextMenu();
            }
            else
            {
                ShowAddNewContextMenu(true);
            }
            IsMouseDown = false;
            return;
        }
        if (CurrentMouseOverItem != null)
        {
            if (SelectedItem != null)
            {
                if (SelectedItem.CanCreateLink(CurrentMouseOverItem))
                {
                    Undo.RecordObject(Data, "Create Link");
                    SelectedItem.CreateLink(SelectedData, CurrentMouseOverItem);
                    Refresh();
                    EditorUtility.SetDirty(Data);
                }
            }
            else
            {
                if (SelectedData.CanCreateLink(CurrentMouseOverItem))
                {
                    Undo.RecordObject(Data, "Create Link");
                    SelectedData.CreateLink(SelectedData, CurrentMouseOverItem);
                    Refresh();
                    EditorUtility.SetDirty(Data);
                }
            }
        }
        else if (CurrentEvent.control)
        {
            if (SelectedItem != null)
            {
                Undo.RecordObject(Data, "RemoveFromDiagram Link");
                SelectedItem.RemoveLink(SelectedData);
                Refresh();
                EditorUtility.SetDirty(Data);
            }
            else if (SelectedData != null)
            {
                Undo.RecordObject(Data, "RemoveFromDiagram Link");
                SelectedData.RemoveLink(null);
                //
                //Selected.BaseTypeName = UFrameAssetManager.DesignerVMAssemblyName;
                Refresh();
                EditorUtility.SetDirty(Data);
            }
        }
        if (DidDrag)
        {
            EditorUtility.SetDirty(Data);
        }
        DidDrag = false;
    }

    public void Execute(IEditorCommand command)
    {
        EditorUtility.SetDirty(Data);
        Undo.RecordObject(Data, command.Title);
        this.ExecuteCommand(command);
        Refresh();
    }

    public IEnumerable<object> ContextObjects
    {
        get
        {
            yield return this;
            if (Data != null)
            {
                yield return Data;
            }
            foreach (var diagramItem in AllSelected)
            {
                yield return diagramItem;
                if (diagramItem.Data != null)
                {
                    yield return diagramItem.Data;
                }
            }
        }
    }
}