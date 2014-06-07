namespace Invert.uFrame.Editor.ElementDesigner
{
    public interface IContextMenuItem
    {
        string Path { get; }
        bool Checked { get; set; }
    }
}