using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public abstract class ManagerInspector<TManaged> : uFrameInspector
{
    private string _compileCompleteCallback;
    private string _createNewText;

//    private bool _createOpen = false;

    public bool _open = false;

    private bool _shouldGeneratePrefabNow = false;
 
    public Type ManagedType
    {
        get
        {
            return typeof(TManaged);
        }
    }

    public Component Target
    {
        get
        {
            return target as Component;
        }
    }

    public void OnCompileComplete()
    {
        OnCompileComplete();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        

        //CreateNewGUI();
    }

    public void Update()
    {
        if (_shouldGeneratePrefabNow)
        {
            if (EditorApplication.isCompiling)
            {
                return;
            }

            _shouldGeneratePrefabNow = false;
            this.GetType().GetMethod(_compileCompleteCallback).Invoke(this, null);
            //_compileCompleteCallback = null;
        }
    }

    public void WaitForCompileToComplete(string complete = "OnCompileComplete")
    {
        _compileCompleteCallback = complete;
        AssetDatabase.Refresh();
        _shouldGeneratePrefabNow = true;
    }

 
    protected abstract bool ExistsInScene(Type itemType);

    protected abstract string GetTypeNameFromName(string name);

    protected virtual void OnAdd(string typeName)
    {
        var go = new GameObject(typeName);
        go.AddComponent(typeName);
        go.transform.parent = Target.transform;
        //Selection.objects = new Object[] { go };
    }

    //protected virtual void TypeListManager()
    //{
    //    var SceneManagers = uFrameUtility.GetDerivedTypes<TManaged>(false, false).OrderBy(p => p.Name).ToArray();

    //    _open = Toggle(typeof(TManaged).Name + "s", _open);
     
    //    if (_open)
    //    {
    //        EditorGUILayout.HelpBox("Select a View to add to the scene.", MessageType.None, true);
    //        EditorGUI.indentLevel += 2;
    //        EditorGUILayout.BeginVertical();

    //        foreach (var View in SceneManagers)
    //        {
    //            EditorGUILayout.BeginHorizontal();

    //            var exists = ExistsInScene(View);
    //            EditorGUILayout.LabelField(View.Name, !exists ? EditorStyles.label : EditorStyles.boldLabel, GUILayout.Width(Screen.width * 0.65f));

    //            //Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/" + prefabName + ".prefab");
    //            //EditorGUIUtility.PingObject(theObjectIwantToSelect);

    //            if (!exists)
    //            {
    //                if (GUILayout.Button("Add", GUILayout.Width(Screen.width * 0.2f)))
    //                {
    //                    OnAdd(View.FullName);
    //                }
    //            }
    //            EditorGUILayout.EndHorizontal();
    //            //GUILayout.Box("",GUI.skin.box, new GUILayoutOption[] {GUILayout.ExpandWidth(true), GUILayout.Height(1)});
    //        }
    //        EditorGUI.indentLevel -= 2;
    //        EditorGUILayout.EndVertical();
    //    }
    //}
}