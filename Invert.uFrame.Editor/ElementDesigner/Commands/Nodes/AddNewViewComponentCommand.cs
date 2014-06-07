using System.Collections.Generic;
using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddNewViewComponentCommand : AddItemCommand<ViewComponentData>
    {
        public override string Title
        {
            get { return "Add New View Component"; }
        }
        public override void Perform(ElementsDiagram item)
        {
            var data = new ViewComponentData()
            {
                Data = item.Data,
                Name = item.Data.GetUniqueName("NewViewComponent"),
                Location = new Vector2(15, 15)
            };
            item.Data.ViewComponents.Add(data);
            data.Location = item.LastMouseDownPosition;
        }
    }

    public class ShowItemCommand : EditorCommand<ElementDesignerData>,
        IToolbarCommand, IDiagramContextCommand, IDynamicOptionsCommand
    {
        public override void Perform(ElementDesignerData item)
        {
            var diagramItem = SelectedOption.Value as IDiagramItem;
            item.CurrentFilter.Locations[diagramItem] = new Vector2(0f, 0f);
        }

        public override string CanPerform(ElementDesignerData item)
        {
            if (item == null) return "Designer Data must not be null";
            return null;
        }

        public ToolbarPosition Position
        {
            get
            {
                return ToolbarPosition.Right;
            }
        }

        public IEnumerable<UFContextMenuItem> GetOptions(object item)
        {
            var designerData = item as ElementDesignerData;
            foreach (var importable in designerData.ImportableItems)
            {
                yield return new UFContextMenuItem()
                {
                    Name = "Show Item/" + importable.Name,
                    Value = importable
                };
            }
            
        }

        public UFContextMenuItem SelectedOption { get; set; }

        public MultiOptionType OptionsType
        {
            get
            {
                return MultiOptionType.DropDown;
            }
        }
    }
}