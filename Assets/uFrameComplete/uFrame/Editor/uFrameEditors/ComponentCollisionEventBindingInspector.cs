using UnityEditor;

[CustomEditor(typeof(CollisionEventBinding), true)]
public class ComponentCollisionEventBindingInspector : ComponentCommandBindingInspector
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (!ViewModelInspector()) return;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_CollisionEvent"));

        serializedObject.ApplyModifiedProperties();
    }
}