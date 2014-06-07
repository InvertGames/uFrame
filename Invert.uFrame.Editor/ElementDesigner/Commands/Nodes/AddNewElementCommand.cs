namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddNewElementCommand : AddItemCommand<ElementData>
    {
        public override string Title
        {
            get { return "Add New Element"; }
        }

        public override void Perform(ElementsDiagram item)
        {
            var data = new ElementData
            {
                Data = item.Data,
                Name = item.Data.GetUniqueName("NewElement"),
                //BaseTypeName = typeof(ViewModel).FullName,
                Dirty = true
            };
            data.Location = item.LastMouseDownPosition;
            data.Filter.Locations[data] = data.Location;
            item.Data.ViewModels.Add(data);
        }
    }
}