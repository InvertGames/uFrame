using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class uFrameInspector : Editor
{
    protected Dictionary<string, FieldInfo> _toggleGroups;
    protected Dictionary<string, List<FieldInfo>> _groupFields;
    private FieldInfo[] _fieldInfos;
    //private GameManager _manager;

    //public GameManager GCManager
    //{
    //    get
    //    {
    //        return _manager ?? (_manager = uFrameUtility.GCManager);
    //    }
    //}

    public static void DrawTitleBar(string subTitle)
    {
        //GUI.Label();
        UFStyles.DoTilebar(subTitle);


    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
     
        //DrawDefaultInspector();
    }

    //public string ViewModelGUI(ViewModel model)
    //{
    //    var properties = model.GetProperties();
    //    foreach (var property in properties)
    //    {
    //        var modelProperty = property._ModelProperty;
    //        if (modelProperty == null) continue;
    //        EditorGUILayout.LabelField(PrettyLabel(property._Name),property._ModelProperty.ObjectValue.ToString());
    //        //if (t == typeof (Vector3))
    //        //{
    //        //    modelProperty.ObjectValue = EditorGUILayout.Vector3Field(PrettyLabel(property._Name), (Vector3)modelProperty.ObjectValue);
    //        //}else
    //        //if (t == typeof(Vector2))
    //        //{
    //        //    modelProperty.ObjectValue = EditorGUILayout.Vector2Field(PrettyLabel(property._Name), (Vector2)modelProperty.ObjectValue);
    //        //}
    //        //if (t == typeof(Vector4))
    //        //{
    //        //    modelProperty.ObjectValue = EditorGUILayout.Vector4Field(PrettyLabel(property._Name), (Vector4)modelProperty.ObjectValue);
    //        //}
    //        //else if (t == typeof (Bounds))
    //        //{
    //        //    modelProperty.ObjectValue = EditorGUILayout.BoundsField(PrettyLabel(property._Name), (Bounds) modelProperty.ObjectValue);
    //        //}
    //        //else if (t == typeof (Color))
    //        //{
    //        //    modelProperty.ObjectValue = EditorGUILayout.ColorField(PrettyLabel(property._Name), (Color) modelProperty.ObjectValue);
    //        //}
    //        // else if (t == typeof (float))
    //        //{
    //        //    modelProperty.ObjectValue = EditorGUILayout.FloatField(PrettyLabel(property._Name), (float) modelProperty.ObjectValue);
    //        //}
    //        // else if (t == typeof (int))
    //        //{
    //        //    modelProperty.ObjectValue = EditorGUILayout.IntField( PrettyLabel(property._Name), (int) modelProperty.ObjectValue);
    //        //}
    //        // else if (t == typeof (Rect))
    //        //{
    //        //    modelProperty.ObjectValue = EditorGUILayout.RectField(PrettyLabel(property._Name), (Rect) modelProperty.ObjectValue);
    //        //}
    //        // else if (t == typeof (string))
    //        //{
    //        //    modelProperty.ObjectValue = EditorGUILayout.TextField(PrettyLabel(property._Name), (string) modelProperty.ObjectValue);
    //        //}
    //        //else if (t.IsEnum)
    //        //{
    //        //   modelProperty.ObjectValue = EditorGUILayout.EnumPopup(PrettyLabel(property._Name),(Enum) modelProperty.ObjectValue);
    //        //}
    //    }

    //    return model.ToString();
    //}

    public string PrettyLabel(string label)
    {
        return Regex.Replace(label, @"[^\w\s]|\_", "");
    }

    //public void EnumField(string label, string value, Action<string> set, params string[] items)
    //{
    //    var targetPropertyIndex = 0;
    //    if (!string.IsNullOrEmpty(value))
    //    {
    //        var obj = items.FirstOrDefault(p => p == value);
    //        if (obj != null)
    //        {
    //            targetPropertyIndex = Array.IndexOf(items, obj);
    //        }
    //    }
    //    EditorGUILayout.EnumPopup()
    //    var newTargetPropertyIndex = EditorGUILayout.Popup(label, targetPropertyIndex, items);
    //    if (newTargetPropertyIndex != targetPropertyIndex)
    //    {
    //        set(items[newTargetPropertyIndex]);
    //    }
    //}
    public string ReflectionPopup(string label, SerializedProperty prop, string[] properties)
    {
        var index = Array.IndexOf(properties, prop.stringValue);
        var newIndex = EditorGUILayout.Popup(label, index, properties);

        if (newIndex != index)
        {
            prop.stringValue = properties[newIndex];
        }
        return prop.stringValue;
    }

    public bool Toggle(string text, bool open)
    {
        var rect = UBEditor.GetRect(UBStyles.SubHeaderStyle);
        var result = GUI.Toggle(rect, open, (open ? "- " : "+ ") + text, UBStyles.SubHeaderStyle);
        if (result) EditorGUILayout.Space();
        return result;
    }

    public void Section(string text)
    {
        var rect = UBEditor.GetRect(UBStyles.SubHeaderStyle);
        GUI.Toggle(rect, true, text, UBStyles.SubHeaderStyle);
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;
    }

    public void EndSection()
    {
        EditorGUI.indentLevel--;
    }

    public void DrawField(object trgt,FieldInfo info)
    {
        //EditorGUI.BeginChangeCheck();
        //object value;
        //if (info.FieldType == typeof (int))
        //{
            
        //}
        //if (EditorGUI.EndChangeCheck())
        //{
        //    info.SetValue(trgt,info);
        //}
    }
    
    protected void GetFieldInformation(ViewBase t)
    {
        _fieldInfos = t.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        _toggleGroups = new Dictionary<string, FieldInfo>();
        _groupFields = new Dictionary<string, List<FieldInfo>>();
        foreach (var fieldInfo in _fieldInfos)
        {
            var attribute = fieldInfo.GetCustomAttributes(typeof(UFToggleGroup), true).FirstOrDefault() as UFToggleGroup;
            var requireInstanceAttribute =
                fieldInfo.GetCustomAttributes(typeof(UFRequireInstanceMethod), true).FirstOrDefault() as UFRequireInstanceMethod;

            if (requireInstanceAttribute != null)
            {
                var method = t.GetType()
                    .GetMethod(requireInstanceAttribute.MethodName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
                if (method == null)
                {
                    var value = (bool)fieldInfo.GetValue(target);
                    if (value)
                    {
                        fieldInfo.SetValue(target, false);
                    }
                    continue;
                }
            }

            if (attribute == null)
            {
                var groupAttribute =
                    fieldInfo.GetCustomAttributes(typeof(UFGroup), true).FirstOrDefault() as UFGroup;
                if (groupAttribute == null) continue;
                if (!_groupFields.ContainsKey(groupAttribute.Name))
                {
                    _groupFields.Add(groupAttribute.Name, new List<FieldInfo>());
                }
                _groupFields[groupAttribute.Name].Add(fieldInfo);
            }
            else
            {
                if (!_toggleGroups.ContainsKey(attribute.Name))
                {
                    _toggleGroups.Add(attribute.Name, fieldInfo);
                }
                if (!_groupFields.ContainsKey(attribute.Name))
                {
                    _groupFields.Add(attribute.Name, new List<FieldInfo>());
                }
            }
        }

    }
}