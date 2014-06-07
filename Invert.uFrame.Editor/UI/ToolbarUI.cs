using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using UnityEditor;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    public class ToolbarUI : ICommandUI
    {
        public ToolbarUI()
        {
            LeftCommands = new List<IEditorCommand>();
            RightCommands = new List<IEditorCommand>();
            AllCommands = new List<IEditorCommand>();
        }

        public List<IEditorCommand> AllCommands { get; set; }

        public List<IEditorCommand> LeftCommands { get; set; }
        public List<IEditorCommand> RightCommands { get; set; }
       

        public void AddCommand(IEditorCommand command)
        {
            AllCommands.Add(command);
            var cmd = command as IToolbarCommand;
            if (cmd == null || cmd.Position == ToolbarPosition.Right)
            {
                RightCommands.Add(command);
            }
            else
            {
                LeftCommands.Add(command);
            }
        }

 

        public void Go()
        {
            foreach (var editorCommand in LeftCommands)
            {
                DoCommand(editorCommand);
            }
            GUILayout.FlexibleSpace();
            var scale = GUILayout.HorizontalSlider(UFStyles.Scale, 0.55f, 1f, GUILayout.Width(200f));
            if (scale != UFStyles.Scale)
            {
                UFStyles.Scale = scale;
                Handler.Execute(new ScaleCommand() {Scale = scale});
                
            }
            foreach (var editorCommand in RightCommands)
            {
                DoCommand(editorCommand);
            }
        }

        public ICommandHandler Handler { get; set; }

        public void DoCommand(IEditorCommand command)
        {

            if (command is IDynamicOptionsCommand)
            {
                var cmd = command as IDynamicOptionsCommand;
                var obj = Handler.ContextObjects.FirstOrDefault(p => cmd.For.IsAssignableFrom(p.GetType()));

                foreach (var ufContextMenuItem in cmd.GetOptions(obj))
                {
                    if (GUILayout.Button(new GUIContent(ufContextMenuItem.Name), EditorStyles.toolbarButton))
                    {
                        cmd.SelectedOption = ufContextMenuItem;
                        Handler.Execute(command);
                    }
                }
            }
            else if (GUILayout.Button(new GUIContent(command.Title), EditorStyles.toolbarButton))
            {

                if (command is IParentCommand)
                {
                    var contextUI = uFrameEditor.CreateCommandUI<ContextMenuUI>(Handler, command.GetType());
                    contextUI.Go();
                }
                else
                {
                    Handler.Execute(command);
                }
            }
        }
    }
}