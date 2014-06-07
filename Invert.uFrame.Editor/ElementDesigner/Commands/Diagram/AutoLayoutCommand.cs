using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner
{
    public class AutoLayoutCommand : ElementsDiagramToolbarCommand
    {
        public override string Title
        {
            get { return "Auto Layout"; }
        }

        public override void Perform(ElementsDiagram item)
        {
            var x = 0f;
            var y = 20f;
            foreach (var viewModelData in item.Data.DiagramItems)
            {
                viewModelData.Location = new Vector2(x, y);
                x += viewModelData.Position.width + 10f;
                //y += viewModelData.Position.height + 10f;
            }
            item.Refresh();
        }
    }
}