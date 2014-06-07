using System;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using UnityEditor;
using UnityEngine;

#if DEBUG
namespace System.Runtime.CompilerServices
{
    public class ExtensionAttribute : Attribute { }
}
#endif

public class ElementsDesigner : EditorWindow, ICommandHandler
{
    private static float HEIGHT = 768;

    private static List<Matrix4x4> stack = new List<Matrix4x4>();

    private static float WIDTH = 1024;

    public ICommandUI Toolbar
    {
        get
        {
            if (_toolbar != null) return _toolbar;


            return _toolbar = uFrameEditor.CreateCommandUI<ToolbarUI>(this, typeof(IToolbarCommand));
        }
        set { _toolbar = value; }
    }

    [SerializeField]
    private ElementsDiagram _diagram;

    private Vector2 _scrollPosition;
    private ICommandUI _toolbar;


    public static ElementDesignerData SelectedElementDiagram
    {
        get { return Selection.activeObject as ElementDesignerData; }
    }

    public ElementsDiagram Diagram
    {
        get { return _diagram; }
        set
        {
            _diagram = value;
            _toolbar = null;
        }
    }

    public float InspectorWidth
    {
        get
        {
            return 0;
            //if (Diagram == null || Diagram.Selected == null)
            //    return 0;
            //return 240;
        }
    }

    public bool IsMiddleMouseDown { get; set; }

    public string LastLoadedDiagram
    {
        get
        {
            return EditorPrefs.GetString("LastLoadedDiagram", null);
        }
        set
        {
            EditorPrefs.SetString("LastLoadedDiagram", value);
        }
    }

    public Vector2 PanStartPosition { get; set; }

    static public void BeginGUI()
    {
        stack.Add(GUI.matrix);
        Matrix4x4 m = new Matrix4x4();
        var w = (float)Screen.width;
        var h = (float)Screen.height;
        var aspect = w / h;
        var scale = 1f;
        var offset = Vector3.zero;
        if (aspect < (WIDTH / HEIGHT))
        { //screen is taller
            scale = (Screen.width / WIDTH);
            offset.y += (Screen.height - (HEIGHT * scale)) * 0.5f;
        }
        else
        { // screen is wider
            scale = (Screen.height / HEIGHT);
            offset.x += (Screen.width - (WIDTH * scale)) * 0.5f;
        }
        m.SetTRS(offset, Quaternion.identity, Vector3.one * scale);
        GUI.matrix *= m;
    }

    static public void EndGUI()
    {
        GUI.matrix = stack[stack.Count - 1];
        stack.RemoveAt(stack.Count - 1);
    }

    [MenuItem("Window/Element Designer", false, 1)]
    public static void Init()
    {
        // Get existing open window or if none, make a new one:
        var window = (ElementsDesigner)GetWindow(typeof(ElementsDesigner));
        window.title = "Element Designer";

        //var repo = new ElementsDataRepository();
        //var diagram = new ElementsDiagram(repo);
        //diagram.Data.ViewModels.Add(repo.GetViewModel(typeof(FPSWeaponViewModel)));
        //diagram.Data.ViewModels.Add(repo.GetViewModel(typeof(FPSBulletViewModel)));

        // RemoveFromDiagram when switching to add all
        window.Show();
    }

    public void InfoBox(string message, MessageType type = MessageType.Info)
    {
        EditorGUI.HelpBox(new Rect(15, 30, 300, 30), message, type);
    }

    public void OnGUI()
    {
        if ((Diagram == null || Diagram.Data == null) && !string.IsNullOrEmpty(LastLoadedDiagram))
        {
            Diagram = null;
            var index = Array.IndexOf(UFrameAssetManager.DiagramNames, LastLoadedDiagram);
            if (index > -1 && index < UFrameAssetManager.Diagrams.Count)
            {
                var lastDiagram = UFrameAssetManager.Diagrams[index];
                if (lastDiagram != null)
                {
                    LoadDiagram(lastDiagram);
                }
            }
        }
        var diagramRect = new Rect(0f, EditorStyles.toolbar.fixedHeight - 1, Screen.width - InspectorWidth, Screen.height - EditorStyles.toolbar.fixedHeight - EditorStyles.toolbar.fixedHeight - 1);

        var style = UFStyles.Background;
        style.border = new RectOffset(
            Mathf.RoundToInt(41),
            Mathf.RoundToInt(41),
            Mathf.RoundToInt(32),
            Mathf.RoundToInt(32));
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        //DoToolbar(diagramRect);
        DoToolbar2();
        GUILayout.EndHorizontal();

        GUI.Box(diagramRect, string.Empty, style);

        if (Diagram == null)
        {
        }
        else
        {
            if (Event.current.control && Event.current.type == EventType.mouseDown)
            {
            }
            _scrollPosition = GUI.BeginScrollView(diagramRect, _scrollPosition, Diagram.DiagramSize);

            HandlePanning(diagramRect);
            if (Diagram.Data != null)
            {
                var x = 0f;
                var every10 = 0;

                while (x < diagramRect.width + _scrollPosition.x)
                {
                    Handles.color = new Color(0f, 0f, 0f, 0.1f);
                    if (every10 == 10)
                    {
                        Handles.color = new Color(0f, 0f, 0f, 0.3f);
                        every10 = 0;
                    }
                    Handles.DrawLine(new Vector2(x, 0f), new Vector2(x, Screen.height + _scrollPosition.y));
                    x += Diagram.Data.SnapSize * UFStyles.Scale;
                    every10++;
                }
                var y = 3f;
                every10 = 0;
                while (y < diagramRect.height + _scrollPosition.y)
                {
                    Handles.color = new Color(0f, 0f, 0f, 0.1f);
                    if (every10 == 10)
                    {
                        Handles.color = new Color(0f, 0f, 0f, 0.3f);
                        every10 = 0;
                    }
                    Handles.DrawLine(new Vector2(0, y), new Vector2(Screen.width + _scrollPosition.x, y));
                    y += Diagram.Data.SnapSize * UFStyles.Scale;
                    every10++;
                }
            }
            //BeginGUI();
            Diagram.Draw();

            //EndGUI();
            GUI.EndScrollView();
        }

        if (EditorApplication.isCompiling)
        {
            InfoBox("Compiling.. Please Wait!");
        }
        if (Diagram != null && Diagram.Data != null)
        {
            var refactors = Diagram.Data.Refactorings.Count;
            if (refactors > 0)
            {
                InfoBox(string.Format("You have {0} refactors. Save before recompiling occurs.", refactors), MessageType.Warning);
            }
        }

        if (Diagram != null && Diagram.Dirty || EditorApplication.isCompiling)
        {
            Repaint();
        }
        if (Event.current.type == EventType.ValidateCommand &&
         Event.current.commandName == "UndoRedoPerformed")
        {
            if (Diagram != null)
                LoadDiagram(Diagram.Data);
        }
    }

    public void OnLostFocus()
    {
        if (Diagram == null) return;
        Diagram.Selected = null;
        Diagram.SelectedItem = null;
        Diagram.IsMouseDown = false;
        Diagram.DeselectAll();
    }

    public virtual void OpenDiagramByAttribute(Type type)
    {
        var attribute = type.GetCustomAttributes(typeof(DiagramInfoAttribute), true).FirstOrDefault() as DiagramInfoAttribute;
        if (attribute == null) return;
        LoadDiagramByName(attribute.DiagramName);
    }

    public void Update()
    {
        if (Diagram == null) return;

        if (Diagram.IsMouseDown || Diagram.Dirty || EditorApplication.isCompiling)
        {
            Repaint();
            Diagram.Dirty = false;
        }

        if (SelectedElementDiagram != null)
        {
            if (Diagram != null)
            {
                if (Diagram.Data != SelectedElementDiagram)
                {
                    LoadDiagram(SelectedElementDiagram);
                }
            }
            else if (Diagram == null || Diagram.Data == null)
            {
                LoadDiagram(SelectedElementDiagram);
            }
        }
    }

    private void DiagramOnSelectionChanged(IDiagramItem diagramItem, IDiagramItem item)
    {
        var behaviourItem = Diagram.SelectedItem as BehaviourSubItem;
        if (behaviourItem != null)
        {
            Selection.activeObject = behaviourItem.Behaviour;
        }
    }

    private void DoToolbar2()
    {
        if (GUILayout.Button(new GUIContent(Diagram == null || Diagram.Data == null ? "--Select Diagram--" : Diagram.Data.name),EditorStyles.toolbarPopup))
        {
            SelectDiagram();
        }
        Toolbar.Go();
    }

    private void HandlePanning(Rect diagramRect)
    {
        if (Event.current.button == 2 && Event.current.type == EventType.MouseDown)
        {
            IsMiddleMouseDown = true;
            PanStartPosition = Event.current.mousePosition;
        }
        if (Event.current.button == 2 && Event.current.rawType == EventType.MouseUp && IsMiddleMouseDown)
        {
            IsMiddleMouseDown = false;
        }
        if (IsMiddleMouseDown)
        {
            var delta = PanStartPosition - Event.current.mousePosition;
            _scrollPosition += delta;
            if (_scrollPosition.x < 0)
                _scrollPosition.x = 0;
            if (_scrollPosition.y < 0)
                _scrollPosition.y = 0;
            if (_scrollPosition.x > diagramRect.width - diagramRect.x)
            {
                _scrollPosition.x = diagramRect.width - diagramRect.x;
            }
            if (_scrollPosition.y > diagramRect.height - diagramRect.y)
            {
                _scrollPosition.y = diagramRect.height - diagramRect.y;
            }
        }
    }

    private void LoadDiagram(string path)
    {
        Diagram = uFrameEditor.Container.Resolve<ElementsDiagram>();
        Diagram = new ElementsDiagram(path);
        Diagram.SelectionChanged += DiagramOnSelectionChanged;
        LastLoadedDiagram = path;

        Diagram.Data.ApplyFilter();
        Diagram.Refresh(true);

        // var newScrollPosition = new Vector2(Diagram.DiagramSize.width, Diagram.DiagramSize.height).normalized / 2;
        //_scrollPosition = new Vector2(250,250);
    }

    private void LoadDiagramByName(string diagramName)
    {
        var diagram = UFrameAssetManager.Diagrams.FirstOrDefault(p => p.Name == diagramName);
        if (diagram == null) return;
        LoadDiagram(diagram);
    }

    private void SelectDiagram()
    {
        var diagramNames = UFrameAssetManager.DiagramNames;
        var menu = new GenericMenu();
        for (int index = 0; index < diagramNames.Length; index++)
        {
            var diagramName = diagramNames[index];
            var diagram = UFrameAssetManager.Diagrams[index];

            menu.AddItem(new GUIContent(diagramName), Diagram != null && diagram == Diagram.Data, () =>
            {
                LastLoadedDiagram = diagramName;
                LoadDiagram(diagram);
                Selection.activeObject = diagram;
            });
        }
        menu.ShowAsContext();
    }

    private void SelectFilter(IDiagramFilter filter1)
    {
        var menu = new GenericMenu();

        var filters = Diagram.Data.GetFilters(filter1).ToArray();
        foreach (var diagramFilter in filters)
        {
            IDiagramFilter filter = diagramFilter;
            menu.AddItem(new GUIContent(diagramFilter.Name), Diagram.Data.CurrentFilter == diagramFilter, () =>
            {
                Diagram.Data.PushFilter(filter);
                Diagram.Refresh(true);
            });
        }
        menu.ShowAsContext();
    }

    public void Execute(IEditorCommand command)
    {
        this.ExecuteCommand(command);
        Diagram.Refresh();
        Repaint();
    }

    public IEnumerable<object> ContextObjects
    {
        get
        {
            yield return this;
            if (Diagram != null)
                yield return Diagram;
            if (Diagram != null && Diagram.Data != null)
                yield return Diagram.Data;
        }
    }
}


