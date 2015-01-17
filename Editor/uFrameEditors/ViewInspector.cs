using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Invert.Common;
using Invert.Common.UI;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;
using State = Invert.StateMachine.State;
using UniRx;
[CustomEditor(typeof(ViewBase), true)]
public class ViewInspector : uFrameInspector
{
    private List<ViewModelCommandInfo> _commands;

    public static bool ShowInfoLabels
    {
        get
        {
            return uFrameEditor.ShowInfoLabels;
        }
        set { uFrameEditor.ShowInfoLabels = value; }
    }

    [MenuItem("Tools/[u]Frame/Info Labels")]
    public static void ShowHideInfoLabels()
    {
        ShowInfoLabels = !ShowInfoLabels;
    }
    public bool ShowIdentifierSettings
    {
        get
        {
            return EditorPrefs.GetBool("UFRAME_ShowIdentifierSettings", true);
        }
        set
        {
            EditorPrefs.SetBool("UFRAME_ShowIdentifierSettings", value);
        }
    }
    public bool ShowDefaultSettings
    {
        get
        {
            return EditorPrefs.GetBool("UFRAME_ShowDefaultSettings", true);
        }
        set
        {
            EditorPrefs.SetBool("UFRAME_ShowDefaultSettings", value);
        }
    }
    public bool ShowViewModelSettings
    {
        get
        {
            return EditorPrefs.GetBool("UFRAME_ShowViewModelSettings", true);
        }
        set
        {
            EditorPrefs.SetBool("UFRAME_ShowViewModelSettings", value);
        }
    }

    public bool ShowViewSettings
    {
        get
        {
            return EditorPrefs.GetBool("UFRAME_ShowViewSettings", true);
        }
        set
        {
            EditorPrefs.SetBool("UFRAME_ShowViewSettings", value);
        }
    }

    public void OnSceneGUI()
    {
        Handles.BeginGUI();
        var padding = 10f;
        var titleContent = new GUIContent(target.name);
        var subTitleContent = new GUIContent(target.GetType().Name);
        var titleSize = ElementDesignerStyles.ViewBarTitleStyle.CalcSize(titleContent);
        var subTitleSize = ElementDesignerStyles.ViewBarSubTitleStyle.CalcSize(subTitleContent);
        var maxTextWidth = Mathf.Max(titleSize.x, subTitleSize.x);
        var barWidth = (padding * 4f) + maxTextWidth + (36 * 1);
        var rect = new Rect(15f, 15f, barWidth, 48f);
        ElementDesignerStyles.DrawExpandableBox(rect, ElementDesignerStyles.SceneViewBar, "");
        GUILayout.BeginArea(rect);

        GUILayout.BeginHorizontal();
        GUILayout.Space(padding);
        if (GUILayout.Button(new GUIContent("", "View " + subTitleContent.text + " in Element Designer"), ElementDesignerStyles.EyeBall))
        {
            uFrameEditorSceneManager.NavigateBack(target as ViewBase);
        }
        GUILayout.Space(padding);
        GUILayout.BeginVertical();
        GUILayout.Space(6f);
        GUILayout.Label(titleContent, ElementDesignerStyles.ViewBarTitleStyle, GUILayout.Width(maxTextWidth));
        GUILayout.Label(subTitleContent, ElementDesignerStyles.ViewBarSubTitleStyle, GUILayout.Width(maxTextWidth));

        GUILayout.EndVertical();
        //GUILayout.Space(padding);
        //if (GUILayout.Button(new GUIContent("", "Move to the previous " + subTitleContent.text), ElementDesignerStyles.NavigatePreviousStyle))
        //{

        //    uFrameEditorSceneManager.NavigatePrevious();
        //}
        //if (GUILayout.Button(new GUIContent("","Move to the next " + subTitleContent.text), ElementDesignerStyles.NavigateNextStyle))
        //{
        //    uFrameEditorSceneManager.NavigateNext();
        //}
        //GUILayout.Space(padding);
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        Handles.EndGUI();
    }

    public void Info(string message)
    {
        if (!ShowInfoLabels) return;
        EditorGUILayout.HelpBox(message, MessageType.Info);
    }
    public void Warning(string message)
    {

        EditorGUILayout.HelpBox(message, MessageType.Warning);
    }
    public override void OnInspectorGUI()
    {
        GUIHelpers.IsInsepctor = true;
        var t = target as ViewBase;
        if (EditorApplication.isPlaying)
        {
           
            ShowDefaultSettings = Toggle("Default", ShowDefaultSettings);
            if (ShowDefaultSettings)
            {
                EditorGUILayout.Space();
                base.OnInspectorGUI();
            }
            DrawPlayModeGui();
            return;
        }

        ShowDefaultSettings = Toggle("Default", ShowDefaultSettings);
        if (ShowDefaultSettings)
        {

            base.OnInspectorGUI();

        }

        serializedObject.Update();
        ShowIdentifierSettings = Toggle("Initialization", ShowIdentifierSettings);
        if (ShowIdentifierSettings)
        {
            DoInitializationSection(t);
        }

        if (_groupFields == null)
            GetFieldInformation(t);

        if (_groupFields != null)
        {

            foreach (var groupField in _groupFields)
            {
                if (_toggleGroups.ContainsKey(groupField.Key)) continue;

                EditorPrefs.SetBool(groupField.Key, Toggle(groupField.Key, EditorPrefs.GetBool(groupField.Key, false)));


                DoGroupField(groupField, t);
            }
            EditorPrefs.SetBool("UFRAME_BindingsOpen", Toggle("Bindings", EditorPrefs.GetBool("UFRAME_BindingsOpen", false)));

            if (EditorPrefs.GetBool("UFRAME_BindingsOpen", false))
            {

                DoBindingsSection();
            }

            //var btnContent = new GUIContent("Show In Designer");
            //if (GUI.Button(GUIHelpers.GetRect(ElementDesignerStyles.ButtonStyle), btnContent, ElementDesignerStyles.ButtonStyle))
            //{
            //    uFrameEditorSceneManager.NavigateBack(target as ViewBase);
            //}
        }


        serializedObject.ApplyModifiedProperties();
        GUIHelpers.IsInsepctor = false;
    }

    private void DoGroupField(KeyValuePair<string, List<FieldInfo>> groupField, ViewBase t)
    {
        if (EditorPrefs.GetBool(groupField.Key, false))
        {

            if (groupField.Key == "View Model Properties" &&
                !(t.OverrideViewModel)) return;
            foreach (var field in groupField.Value)
            {
                try
                {
                    // serializedObject.GetIterator().Reset();
                    var property = serializedObject.FindProperty(field.Name);
                    if (property == null) continue;
                    if (property.propertyType == SerializedPropertyType.Vector2)
                    {
                        var newValue = EditorGUILayout.Vector2Field(property.name, property.vector2Value);
                        if (newValue != property.vector2Value)
                        {
                            property.vector2Value = newValue;
                        }
                    }
                    else if (property.propertyType == SerializedPropertyType.Vector3)
                    {
                        var newValue = EditorGUILayout.Vector3Field(property.name, property.vector3Value);
                        if (newValue != property.vector3Value)
                        {
                            property.vector3Value = newValue;
                        }
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(property);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log(field.Name + ex.Message);
                }
            }
        }
    }

    private void DoBindingsSection()
    {
        foreach (var group in _toggleGroups)
        {
            var property = serializedObject.FindProperty(@group.Value.Name);
            EditorGUILayout.PropertyField(property, new GUIContent(property.name.Replace("_", "").Replace("Bind", "")));
            if (property.boolValue)
            {
                EditorGUI.indentLevel++;
                if (_groupFields != null)
                {
                    if (_groupFields.ContainsKey(@group.Key))
                    {
                        foreach (var groupField in _groupFields[@group.Key])
                        {
                            var subProperty = serializedObject.FindProperty(groupField.Name);

                            if (subProperty != null)
                            {
                                EditorGUILayout.PropertyField(subProperty,
                                    new GUIContent(subProperty.name.Replace(@group.Key, "").Replace("_", "")));
                            }
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }
            property.Reset();
        }
    }

    private void DoInitializationSection(ViewBase t)
    {
        EditorGUILayout.Space();
        var resolveProperty = serializedObject.FindProperty("_forceResolveViewModel");
        var resolveNameProperty = serializedObject.FindProperty("_resolveName");

        if (!string.IsNullOrEmpty(t.DefaultIdentifier) && (resolveNameProperty.stringValue != t.DefaultIdentifier || !resolveProperty.boolValue))
            if (GUILayout.Button(string.Format("Use Registered \"{0}\" Instance", t.DefaultIdentifier)))
            {
                resolveProperty.boolValue = true;
                serializedObject.FindProperty("_resolveName").stringValue = t.DefaultIdentifier;
            }

        Info("Store the ViewModel instance in the container.  So other views can use the same instance.");

        EditorGUILayout.PropertyField(resolveProperty, new GUIContent("Force Resolve"));


        Info(
            "The Identifier is used for persisting this view and should be unique.  This identifier should always be the same each time the scene loads.  If instantiating prefabs you'll want to override the 'Identifier' property and make it unique.");
        if (t != null)
            EditorGUILayout.LabelField("Id", t.Identifier);

        if (t.ForceResolveViewModel)
        {
            Info("The name that is used to share this instance among others (if any).");

            EditorGUILayout.PropertyField(resolveNameProperty, new GUIContent("Resolve Name"));
            //if (!t.IsMultiInstance && !string.IsNullOrEmpty(resolveNameProperty.stringValue))
            //{
            //    Warning(
            //        "When using a 'ResolveName' on a single instance element, the element must be initialized manually in the scene manager's setup method.");
            //}
        }


        var saveProperty = serializedObject.FindProperty("_Save");
        Info(
            "Should this field be saved and loaded when saving a scene's in-game state. e.g. GameManager.ActiveSceneManager.Load(...); GameManger.ActiveSceneManager.Save(...);");
        EditorGUILayout.PropertyField(saveProperty, new GUIContent("Save & Load"));

        var injectProperty = serializedObject.FindProperty("_InjectView");
        Info(
            "Should this view be injected with Dependencies defined in the GameContainer.  e.g.GameManager.Resolve<MyViewModel>(ResolveName);");
        EditorGUILayout.PropertyField(injectProperty, new GUIContent("Inject This View"));

        //var useHashCode = serializedObject.FindProperty("_UseHashcodeAsIdentifier");
        //Info(
        //    "Should this view use it's hash code as it's unique identifier.  Use if this is a prefab that will be place in a scene through the editor.");
        //EditorGUILayout.PropertyField(useHashCode, new GUIContent("Use Hash As Identifier"));

        Info(
                   "This should always be checked except when you are instantiating it manually, or its using a shared instance that is already being initialized.");
        var overrideProperty = serializedObject.FindProperty("_overrideViewModel");
        EditorGUILayout.PropertyField(overrideProperty, new GUIContent("Initialize ViewModel"));
    }

    public ViewBase Target
    {
        get { return (ViewBase)target; }
    }
    public List<ViewModelCommandInfo> Commands
    {
        get
        {
            if (_commands == null)
            {
                if (Target.ViewModelObject != null)
                {
                    _commands = Target.ViewModelObject.GetViewModelCommands();
                }
            }
            return _commands;
        }
    }
    private void DrawPlayModeGui()
    {
        if (EditorApplication.isPlaying)
        {
            var t = target as ViewBase;
            if (t != null && t.ViewModelObject != null)
            {
                if (GUIHelpers.DoToolbarEx("View Model Properties"))
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Id", t.ViewModelObject.Identifier);
                    EditorGUILayout.LabelField("# References", t.ViewModelObject.References.ToString());
                    DoViewModelGUI(t.ViewModelObject);

                    //foreach (var p in t.ViewModelObject.Properties)
                    //{
                    //    var serialized = p.Value.Serialize().ToString();
                    //    if (serialized.Length > 100)
                    //    {
                    //        serialized = serialized.Substring(0, 100);
                    //    }
                    //    EditorGUILayout.LabelField(p.Key.Replace("_", "").Replace("Property", ""), serialized);
                    //    //if (p.Value.ValueType.IsPrimitive)
                    //    //{
                    //    //    EditorGUILayout.LabelField(p.Key, p.Value.ObjectValue.ToString());
                    //    //}
                    //    //else
                    //    //{
                    //    //    EditorGUILayout.LabelField(p.Key, p.Value.Serialize());
                    //    //}
                    //}
                }

                if (Commands != null)
                {
                    if (GUIHelpers.DoToolbarEx("Commands"))
                    {

                        foreach (var command in Commands)
                        {
                            var type = command.ParameterType;

                            if (type != null && type != typeof (void))
                            {
                                EditorGUI.BeginChangeCheck();
                                object newValue = null;
                                object currentValue = ((IParameterCommand)command.Command).Parameter ?? GetDefaultValue(type);//Activator.CreateInstance(type);

                                var propertyName = "Parameter";
                                var isEnum = false;
                                newValue = DoTypeInspector(type, propertyName, currentValue, isEnum);

                                if (EditorGUI.EndChangeCheck())
                                {
                                    ((IParameterCommand) command.Command).Parameter = newValue;
                                }
                            }
                            if (GUI.Button(GUIHelpers.GetRect(ElementDesignerStyles.ButtonStyle), command.Name,
                                ElementDesignerStyles.ButtonStyle))
                            {
                                GameManager.CommandDispatcher.ExecuteCommand(command.Command, ((IParameterCommand)command.Command).Parameter);
                            }
                        }
                        //foreach (var command in Commands)
                        //{
                        //    if (GUI.Button(GUIHelpers.GetRect(ElementDesignerStyles.ButtonStyle), command.Key,
                        //        ElementDesignerStyles.ButtonStyle))
                        //    {
                        //        var commandWith = command.Value as ICommandWith<object>;



                        //        Target.ExecuteCommand(command.Value);
                        //    }
                        //}
                    }


                }
                //if (t.ViewModelObject != null)
                //{
                //    foreach (var item in t.ViewModelObject.Bindings)
                //    {
                //        if (GUIHelpers.DoToolbarEx(item.Key == -1
                //            ? "Controller"
                //            : EditorUtility.InstanceIDToObject(item.Key).name))
                //        {
                //            foreach (var binding in item.Value)
                //            {

                //                if (GUIHelpers.DoTriggerButton(new UFStyle()
                //                {
                //                    Label = binding.GetType().Name + ": " + binding.ModelMemberName,
                //                    //IconStyle = bi
                //                }))
                //                {

                //                }
                //            }

                //        }


                //    }
                //}
            }
            else
            {
                EditorGUILayout.HelpBox("View Model not initialized yet.", MessageType.Info);
                Repaint();
            }

        }
        Repaint();
        return;
    }

    private static void DoViewModelGUI(ViewModel t)
    {
        if (t == null) return;

        var properties = t.GetViewModelProperties();
        foreach (var property in properties)
        {
            var type = property.Property.ValueType;
            DoViewModelProperty(type, property);
        }
    }

    private static void DoViewModelProperty(Type type, ViewModelPropertyInfo property)
    {

        if (property.IsCollectionProperty)
        {
            if (property.Property != null && property.Property.ObjectValue != null)
            EditorGUILayout.LabelField(property.Property.PropertyName, ((IList)property.Property.ObjectValue).Count.ToString());
            return;
        }
        if (property.IsComputed)
        {
            if (property.Property != null && property.Property.ObjectValue != null)
            EditorGUILayout.LabelField(property.Property.PropertyName, property.Property.ObjectValue.ToString());
            return;
        }
        else if (property.Property.ValueType == typeof(State))
        {
            EditorGUILayout.LabelField(property.Property.PropertyName, property.Property.ObjectValue.ToString());
            var _designerWidnow = uFrameEditor.DesignerWindow;
            if (_designerWidnow != null && _designerWidnow.DiagramDrawer != null)
            {
                var items = _designerWidnow.DiagramDrawer.DiagramViewModel.GraphItems;

                var objectValue = property.Property.ObjectValue as State;
                if (objectValue != null)
                {
                    var stateMachine = objectValue.StateMachine;

                    foreach (var item in items)
                    {
                        var stateNode = item as StateMachineStateNodeViewModel;
                        if (stateNode != null)
                        {
                            stateNode.IsCurrentState = item.Name == objectValue.Name;
                        }
                        else if (stateMachine.LastTransition != null)
                        {
                            var connectionNode = item as ConnectionViewModel;
                            if (connectionNode != null)
                            {

                                connectionNode.IsActive = stateMachine.LastTransition.Name ==
                                                          connectionNode.ConnectorA.ConnectorFor.Name;
                            }
                        }


                    }
                }


                _designerWidnow.Repaint();
            }

            return;
        }
        EditorGUI.BeginChangeCheck();
        object newValue = null;
        object currentValue = property.Property.ObjectValue;
        var propertyName = property.Property.PropertyName;
        var isEnum = property.IsEnum;
        newValue = DoTypeInspector(type,  propertyName, currentValue, isEnum);

        if (EditorGUI.EndChangeCheck())
        {

            property.Property.ObjectValue = newValue;
        }
    }

    private static object DoTypeInspector(Type type,  string propertyName, object currentValue, bool isEnum)
    {
        if (currentValue == null)
        {
            return null;
        }
        object newValue = null;
        if (type == typeof (int))
        {
            newValue = EditorGUILayout.IntField(propertyName,
                (int) currentValue);
        }
        else if (type == typeof (bool))
        {
            newValue = EditorGUILayout.Toggle(propertyName,
                (bool) currentValue);
        }
        else if (type == typeof (string))
        {
            newValue = EditorGUILayout.TextField(propertyName,
                (string) currentValue);
        }
        else if (type == typeof (decimal))
        {
            newValue = Convert.ToDecimal(EditorGUILayout.FloatField(propertyName,
                Convert.ToSingle(currentValue)));
        }
        else if (type == typeof (float))
        {
            newValue = EditorGUILayout.FloatField(propertyName,
                (float) currentValue);
        }
        else if (type == typeof (Vector2))
        {
            newValue = EditorGUILayout.Vector2Field(propertyName,
                (Vector2) currentValue);
        }
        else if (type == typeof (Vector3))
        {
            newValue = EditorGUILayout.Vector3Field(propertyName,
                (Vector3) currentValue);
        }
        else if (type == typeof (Rect))
        {
            newValue = EditorGUILayout.RectField(propertyName,
                (Rect) currentValue);
        }
        else if (type == typeof (Color))
        {
            newValue = EditorGUILayout.ColorField(propertyName,
                (Color) currentValue);
        }
        else if (isEnum)
        {
            newValue = EditorGUILayout.EnumPopup(propertyName,
                (Enum) currentValue);
        }
        return newValue;
    }

    object GetDefaultValue(Type t)
    {
        if (t == typeof (string)) return String.Empty;
        if (t.IsValueType)
            return Activator.CreateInstance(t);

        return null;
    }

}




//public class WireHighlighter : DiagramPlugin
//{
//    public override void Initialize(uFrameContainer container)
//    {
//        EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
//    }

//    private void PlaymodeStateChanged()
//    {
//        if (EditorApplication.isPlaying)
//        {
            
//        }
        

//        EditorApplication.playmodeStateChanged -= PlaymodeStateChanged;
//    }
//}