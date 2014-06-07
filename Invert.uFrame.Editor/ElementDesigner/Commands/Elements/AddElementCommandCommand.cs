using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddElementCommandCommand : EditorCommand<ElementData>
    {
        public override void Perform(ElementData item)
        {
            item.Commands.Add(new ViewModelCommandData()
            {
                Name = item.Data.GetUniqueName("NewCommand")
            });
        }

        public override string CanPerform(ElementData item)
        {
            if (item == null) return "Arg can't be null";
            return null;
        }
    }
}
