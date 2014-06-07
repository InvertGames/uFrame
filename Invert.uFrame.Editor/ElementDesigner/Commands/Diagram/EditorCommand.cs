using System;
using System.Collections.Generic;

namespace Invert.uFrame.Editor.ElementDesigner
{
    public abstract class EditorCommand : IEditorCommand
    {
        private List<CommandHook> _hooks;
        private string _title;


        public abstract Type For { get; }

        public void Execute(object item)
        {
            Perform(item);
        }

        public abstract void Perform(object arg);

        public List<CommandHook> Hooks
        {
            get { return _hooks ?? (_hooks = new List<CommandHook>()); }
            set { _hooks = value; }
        }

        public virtual string Name
        {
            get { return this.GetType().Name.Replace("Command",""); }
        }

        public virtual string Title
        {
            get
            {
                if (string.IsNullOrEmpty(_title))
                {
                    return Name;
                }
                return _title;
            }
            set { _title = value; }
        }

        public string CanExecute(object arg)
        {
            return null;
        }

        public virtual decimal Order { get { return 0; } }
        public abstract string CanPerform(object arg);
        public virtual string Path { get { return Name; } }
        public bool Checked { get; set; }
    }
}