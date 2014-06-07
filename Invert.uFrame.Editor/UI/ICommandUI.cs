using Invert.uFrame.Editor.ElementDesigner;

namespace Invert.uFrame.Editor
{
    public interface ICommandUI
    {

        //void Initialize();
        //void DoSingleCommand(IEditorCommand command, object[] contextObjects, UFContextMenuItem item = null);
        //void DoMultiCommand(IEditorCommand parentCommand, IEditorCommand[] childCommands, object[] contextObjects);
        void AddCommand(IEditorCommand command);
        void Go();
        ICommandHandler Handler { get; set; }
    }
}