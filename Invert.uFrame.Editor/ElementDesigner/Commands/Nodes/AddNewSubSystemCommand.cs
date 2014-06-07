using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddNewSubSystemCommand : AddItemCommand<SubSystemData>
    {
        public override string Title
        {
            get { return "Add New Subsystem"; }
        }
        public override void Perform(ElementsDiagram item)
        {
            var data = new SubSystemData()
            {
                Data = item.Data,
                Name = item.Data.GetUniqueName("New Sub System"),
                Location = new Vector2(15, 15)
            };
            item.Data.SubSystems.Add(data);
            data.Location = item.LastMouseDownPosition;
        }
    }
}