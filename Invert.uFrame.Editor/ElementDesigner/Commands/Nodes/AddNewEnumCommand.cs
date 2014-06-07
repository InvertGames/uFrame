using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddNewEnumCommand : AddItemCommand<EnumData>
    {
        public override string Title
        {
            get { return "Add New Enum"; }
        }

        public override void Perform(ElementsDiagram item)
        {
            var data = new EnumData()
            {
                Data = item.Data,
                Name = item.Data.GetUniqueName("NewEnum"),
                Location = new Vector2(15, 15)
            };
            item.Data.Enums.Add(data);
            data.Location = item.LastMouseDownPosition;
        }
    }
}