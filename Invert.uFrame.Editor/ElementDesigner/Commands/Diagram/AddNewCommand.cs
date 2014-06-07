using System.Collections.Generic;
using Invert.uFrame.Editor.ElementDesigner.Commands;

namespace Invert.uFrame.Editor.ElementDesigner
{
    public class AddNewCommand : ElementsDiagramToolbarCommand, IParentCommand,IDiagramContextCommand
    {
        public override string Title
        {
            get { return "Add New"; }
        }

        public override void Perform(ElementsDiagram item)
        {
            // No implementation
        }
    }

    public abstract class ElementsDiagramToolbarCommand : ToolbarCommand<ElementsDiagram>
    {
        public override string CanPerform(ElementsDiagram item)
        {
            if (item == null) return "No Diagram Open";
            return null;
        }
    }
}