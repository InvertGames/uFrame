using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddNewViewCommand : AddItemCommand<ViewData>
    {
        public override string Title
        {
            get { return "Add New View"; }
        }
        public override void Perform(ElementsDiagram item)
        {
            var data = new ViewData()
            {
                Data = item.Data,
                Name = item.Data.GetUniqueName("NewView"),
                Location = new Vector2(15, 15)
            };
            item.Data.Views.Add(data);
            data.Location = item.LastMouseDownPosition;
        }
    }
}