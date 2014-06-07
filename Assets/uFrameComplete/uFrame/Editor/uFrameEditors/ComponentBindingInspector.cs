using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

[CustomEditor(typeof(ComponentBinding), true)]
public class ComponentBindingInspector : uFrameInspector
{
    public override void OnInspectorGUI()
    {
        ViewModelInspector();

        serializedObject.ApplyModifiedProperties();
    }

    public virtual bool ViewModelInspector()
    {
        var t = target as ComponentBinding;

        var view = t._SourceView ?? t.GetComponent<ViewBase>();

        var viewNew = EditorExtensions.ComponentField("View:", t._SourceView, typeof(ViewBase)) as ViewBase;
        if (!(viewNew == t._SourceView))
        {
            t._SourceView = viewNew;
        }

        if (viewNew == null)
        {
            EditorGUILayout.HelpBox("There is not a view on this game object.", MessageType.Error);
            return false;
        }

        if (t._SourceView == null)
        {
            t._SourceView = view;
        }

        ModelMemberSelection(view as ViewBase, t);
        return true;
    }

    protected virtual void ModelMemberSelection(ViewBase view, ComponentBinding t)
    {
        if (view == null) return;

        var props = ViewModel.GetReflectedModelProperties(view.ViewModelType);

        var property = serializedObject.FindProperty("_ModelMemberName");
        ReflectionPopup("ViewModel Member", property, props.Select(p => p.Key).ToArray());

        //uFrameUtility.PopupField("Model Property", t._ModelMemberName, v =>
        //{
        //    t._ModelMemberName = v;
        //}, propsArray);
    }
}