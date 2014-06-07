using System.Collections.Generic;

namespace Invert.uFrame.Editor.ElementDesigner
{
    public class PopToFilterCommand : ElementsDiagramToolbarCommand, IDynamicOptionsCommand
    {

        public override void Perform(ElementsDiagram item)
        {

            item.Data.PopToFilter(SelectedOption.Name);
            item.Refresh(true);
        }

        public IEnumerable<UFContextMenuItem> GetOptions(object arg)
        {
            var item = arg as ElementsDiagram;
            if (item == null)
            {
                yield break;
            }
            
            yield return new UFContextMenuItem()
            {
                Name = item.Data.SceneFlowFilter.Name, 
                Checked = item.Data.CurrentFilter == item.Data.SceneFlowFilter
            };
            foreach (var filter in item.Data.FilterPath)
            {
                yield return new UFContextMenuItem()
                {
                    Name = filter.Name,
                    Checked = item.Data.CurrentFilter == filter
                };
            }
        }

        public override ToolbarPosition Position
        {
            get { return ToolbarPosition.Left; }
        }

        public UFContextMenuItem SelectedOption { get; set; }
        public MultiOptionType OptionsType { get{ return MultiOptionType.Buttons; } }
    }
}