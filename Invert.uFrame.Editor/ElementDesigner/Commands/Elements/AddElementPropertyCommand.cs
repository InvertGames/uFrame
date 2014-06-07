namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddElementPropertyCommand : EditorCommand<ElementData>
    {
        public override void Perform(ElementData item)
        {
            item.Properties.Add(new ViewModelPropertyData()
            {
                DefaultValue = string.Empty,
                Name = item.Data.GetUniqueName("String1"),
                Type = typeof(string)
            });
        }

        public override string CanPerform(ElementData item)
        {
            if (item == null) return "Arg can't be null";
            return null;
        }
    }
}