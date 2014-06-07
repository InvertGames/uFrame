namespace Invert.uFrame.Editor.ElementDesigner
{
    public abstract class ToolbarCommand<TFor> : EditorCommand<TFor>, IToolbarCommand
    {
        public virtual ToolbarPosition Position { get { return ToolbarPosition.Right; } }
        public virtual decimal Order { get { return 0; } }

        
    }
}