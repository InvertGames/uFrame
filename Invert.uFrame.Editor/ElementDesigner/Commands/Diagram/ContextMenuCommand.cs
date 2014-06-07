using System.Collections.Generic;

namespace Invert.uFrame.Editor.ElementDesigner
{
    public abstract class ContextMenuCommand<TFor> : EditorCommand<TFor>
    {
        public abstract IEnumerable<UFContextMenuItem> GetMenuOptions();
    }
}