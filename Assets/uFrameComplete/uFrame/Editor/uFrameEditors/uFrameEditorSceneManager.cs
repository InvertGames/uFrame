using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public class uFrameEditorSceneManager
{
    private static int managerId;

    private static List<int> markedGames = new List<int>();
    private static List<int> markedViews = new List<int>();
    
    public static SceneManager[] SceneManagers { get; set; }

    public static ViewBase[] SceneViews { get; set; }

    public static Type CurrentFocusType { get; set; }

    public static IEnumerable<ViewBase> FocusableViews
    {
        get
        {
            if (CurrentFocusType != null)
            foreach (var sceneView in SceneViews)
            {
                if (CurrentFocusType.IsAssignableFrom(sceneView.GetType()))
                {
                    yield return sceneView;
                }
            }
        }
    }

    public static void NavigateToFirstView(Type type)
    {
        if (SceneViews == null) return;
        
        var first = SceneViews.FirstOrDefault(p => p.GetType() == type);
        if (first != null)
        {
            NavigateToView(first);
        }
    }

    public static void NavigateToView(ViewBase view)
    {
        if (view == null) return;
        CurrentFocusType = view.GetType();
        var sceneView= EditorWindow.GetWindow<SceneView>();
        sceneView.Show(true);
        Selection.activeGameObject = view.gameObject;
        sceneView.LookAt(view.transform.position);
        sceneView.MoveToView(view.transform);
        CurrentView = view;

    }

    public static int CurrentViewIndex { get; set; }

    public static ViewBase CurrentView { get; set; }

    private static void RefreshSceneObjects()
    {
        var gcm = (GameManager)Object.FindObjectOfType(typeof(GameManager));
        var objs = Object.FindObjectsOfType(typeof(ViewModelObserver));

        if (gcm != null)
            managerId = gcm.gameObject.GetInstanceID();

        SceneViews = objs.OfType<ViewBase>().ToArray();
        markedViews = SceneViews.Select(p => p.gameObject.GetInstanceID()).ToList();
        SceneManagers = objs.OfType<SceneManager>().ToArray();
        markedGames = SceneManagers.Select(p => p.gameObject.GetInstanceID()).ToList();
    }

    #region Editor Icons
    private static Texture2D textureGameManager;
    private static Texture2D textureSceneManager;
    private static Texture2D textureView;

    static uFrameEditorSceneManager()
    {
        // Init
        //SetSearchFilter(string searchFilter, SearchableEditorWindow.SearchMode searchMode, bool setAll)
        
        //texture = Resources.Load("Controller.png") as Texture2D;
        //AssetDatabase.LoadAssetAtPath("Assets/Images/Testicon.png", typeof(Texture2D)) as Texture2D;
        textureGameManager = UFStyles.GetSkinTexture("GameManager");
        textureSceneManager = UFStyles.GetSkinTexture("SceneManager");
        textureView = UFStyles.GetSkinTexture("View");
        EditorApplication.update += RefreshSceneObjects;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
        
    }

    private static void HierarchyItemCB(int instanceID, Rect selectionRect)
    {

        if (selectionRect.width < 265)
        {
            return;
        }

        // place the icoon to the right of the list:
        Rect r = new Rect(selectionRect);
        r.x = r.width - 80;
        r.width = 75;
        r.height = 16;

        if (instanceID == managerId)
        {
            if (textureGameManager != null)
                GUI.DrawTexture(r, textureGameManager);
            //GUI.Label(r, textureGameManager);
        }
        if (markedGames.Contains(instanceID))
        {
            if (textureSceneManager != null)
                GUI.DrawTexture(r, textureSceneManager);
            // Draw the texture if it's a light (e.g.)
            //GUI.Label(r, textureSceneManager);
        }
        if (markedViews.Contains(instanceID))
        {
            if (textureView != null)
                GUI.DrawTexture(r, textureView);
            // Draw the texture if it's a light (e.g.)
            //GUI.Label(r, textureView);
        }
    }

    #endregion

    public static void NavigateBack(ViewBase view)
    {
        var designerWindow = EditorWindow.GetWindow<ElementsDesigner>();
        designerWindow.OpenDiagramByAttribute(view.ViewModelType);

        //designerWindow.Diagram.Data.PushFilter();
    }

    public static void NavigatePrevious()
    {
        
    }

    public static void NavigateNext()
    {
        
    }
}