using System.Linq;

public class ComponentCommandBindingInspector : ComponentBindingInspector
{
    protected override void ModelMemberSelection(ViewBase view, ComponentBinding t)
    {
        //base.ModelMemberSelection(view, t);
        var props = ViewModel.GetReflectedCommands(view.ViewModelType);

        var propsArray = props.Select(p => p.Key).ToArray();
        var property = serializedObject.FindProperty("_ModelMemberName");
        ReflectionPopup("ViewModel Member", property, propsArray);

        // uFrameUtility.PopupField("Model Command", t._ModelMemberName, v => t._ModelMemberName = v, propsArray);
    }
}