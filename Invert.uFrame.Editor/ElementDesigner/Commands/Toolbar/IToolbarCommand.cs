namespace Invert.uFrame.Editor.ElementDesigner
{
    public interface IToolbarCommand : IEditorCommand
    {
        ToolbarPosition Position { get; }
    }
}