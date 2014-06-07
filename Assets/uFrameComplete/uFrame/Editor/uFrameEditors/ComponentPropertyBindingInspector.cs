using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UFPropertyBinding))]
public class ComponentPropertyBindingInspector : ComponentBindingInspector
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var t = target as UFPropertyBinding;

        if (!base.ViewModelInspector())
        {
            return;
        }

        var component = EditorExtensions.ComponentField("Target Component:", t._TargetComponent);
        if (!(component == t._TargetComponent))
        {
            t._TargetComponent = component;
        }

        if (t._TargetComponent == null)
        {
            serializedObject.ApplyModifiedProperties();
            return;
        }

        var arrayFieldName = "_TargetProperties";

        int size = serializedObject.FindProperty(arrayFieldName + ".Array.size").intValue;
        int newSize = EditorGUILayout.IntField("Members Size", size);

        if (newSize != size)
            serializedObject.FindProperty(arrayFieldName + ".Array.size").intValue = newSize;

        EditorGUI.indentLevel = 1;

        object currentMember = t._TargetComponent;

        for (int i = 0; i < newSize; i++)
        {
            var prop = serializedObject.FindProperty(string.Format("{0}.Array.data[{1}]", arrayFieldName, i));
            if (prop == null)
            {
                break;
            }
            else
            {
                if (currentMember == null) break;
                var properties = currentMember.GetType().GetMembers().Where(p => p is PropertyInfo || p is FieldInfo).Select(p => p.Name).OrderBy(p => p).ToArray();
                var nextMemberName = ReflectionPopup(currentMember.GetType().Name, prop, properties);

                var nextProperty = currentMember.GetType().GetProperty(nextMemberName);
                if (nextProperty != null)
                {
                    currentMember = nextProperty.GetValue(currentMember, null);
                }
            }
        }
        if (t._TargetProperties.Contains(null) || t._TargetProperties.Contains(string.Empty))
        {
            EditorGUILayout.HelpBox("All members must be set.", MessageType.Error);
        }
        serializedObject.ApplyModifiedProperties();
    }
}