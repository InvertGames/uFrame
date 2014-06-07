using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UFrameBehaviours))]
public class UFrameBehaviourEditor : TBehavioursEditor
{
    [MenuItem("Assets/[u]Frame/New uFrame Behaviour", false, 42)]
    public static void CreateUBehaviour()
    {
        UBAssetManager.CreateAsset<UFrameBehaviours>();
    }

    public override void DoSettings(IUBehaviours behaviour)
    {
        base.DoSettings(behaviour);
        if (SettingsOpen)
        {
            var viewModelTypeProperty = serializedObject.FindProperty("_ViewModelType");
            //var derived = .ToArray();
            var type = Type.GetType(viewModelTypeProperty.stringValue);
            var typeName = type == null ? "-- Set ViewModel Type --" : type.Name;

            if (GUILayout.Button(typeName))
            {
                UBTypesWindow.Init("View Models", ActionSheetHelpers.GetDerivedTypes<ViewModel>(false, false), (n, t) =>
                {
                    viewModelTypeProperty.serializedObject.Update();
                    viewModelTypeProperty.stringValue = t;
                    viewModelTypeProperty.serializedObject.ApplyModifiedProperties();

                });
                
            }
          
            //var derivedFullNames = derived.Select(p => p.FullName).ToArray();
            //var selectedIndex = Array.IndexOf(derivedFullNames, viewModelTypeProperty.stringValue);

            //var newIndex = EditorGUILayout.Popup("View Model Type:", selectedIndex, derivedFullNames);
            //if (newIndex != selectedIndex)
            //{
            //    viewModelTypeProperty.stringValue = derivedFullNames[newIndex];
            //}
        }
    }

    protected override void DoAdditionalGUI()
    {
        base.DoAdditionalGUI();
        
    }
}