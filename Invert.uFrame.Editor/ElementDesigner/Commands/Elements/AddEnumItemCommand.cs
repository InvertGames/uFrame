namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddEnumItemCommand : EditorCommand<EnumData>
    {
        public override void Perform(EnumData item)
        {
            item.EnumItems.Add(new EnumItem()
            {
                Name = item.Data.GetUniqueName("Item")
            });
        }

        public override string CanPerform(EnumData item)
        {
            if (item == null) return "Arg can't be null";
            return null;
        }

    }
}