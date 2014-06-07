using UnityEditor;

[CustomEditor(typeof(KeyBinding), true)]
public class ComponentKeyBindingInspector : ComponentCommandBindingInspector
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (!ViewModelInspector()) return;
       
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_Shift"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_Alt"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_Control"));
        var keyProperty = serializedObject.FindProperty("_Key");
        EditorGUILayout.PropertyField(keyProperty);
     
        serializedObject.ApplyModifiedProperties();
    }
}