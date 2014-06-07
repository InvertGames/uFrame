using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor.ElementDesigner;
using UnityEditor;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    public class ContextMenuUI : ICommandUI
    {
        public bool Flatten { get; set; }
        public List<IEditorCommand> Commands { get; set; }

        public ContextMenuUI()
        {
            Commands = new List<IEditorCommand>();
        }

        public void AddCommand(IEditorCommand command)
        {
            Commands.Add(command);
        }

        public void CreateMenuItems(GenericMenu genericMenu)
        {

            foreach (var editorCommand in Commands)
            {

                IEditorCommand command = editorCommand;
                var argument = Handler.ContextObjects.FirstOrDefault(p => command.For.IsAssignableFrom(p.GetType()));

                var dynamicCommand = command as IDynamicOptionsCommand;
                if (dynamicCommand != null)
                {
                    foreach (var option in dynamicCommand.GetOptions(argument))
                    {
                        UFContextMenuItem option1 = option;
                        genericMenu.AddItem(new GUIContent(Flatten ? editorCommand.Title : option.Name), option.Checked, () =>
                        {
                            dynamicCommand.SelectedOption = option1;
                            Handler.Execute(command);
                        });
                    }
                }
                else
                {
                    if (command.CanPerform(argument) != null)
                    {
                        genericMenu.AddDisabledItem(new GUIContent(Flatten ? editorCommand.Title : editorCommand.Path));
                    }
                    else
                    {
                        genericMenu.AddItem(new GUIContent(editorCommand.Path), editorCommand.Checked, () =>
                        {
                            
                            Handler.Execute(command);
                        });
                    }
                }
                
                
             
                
            }
        }
        public void Go()
        {
            var genericMenu = new GenericMenu();
            CreateMenuItems(genericMenu);
            genericMenu.ShowAsContext();
        }

        public ICommandHandler Handler { get; set; }
    }
}