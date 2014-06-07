using System.Collections.Generic;
using Invert.uFrame.Editor.ElementDesigner;

namespace Invert.uFrame.Editor
{
    public interface ICommandHandler
    {
        void Execute(IEditorCommand command);
        IEnumerable<object> ContextObjects { get; }
    }
}