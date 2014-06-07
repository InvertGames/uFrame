using System;

namespace Invert.uFrame.Editor.ElementDesigner
{
    public class CommandHook
    {
        public CommandHookLifetime Lifetime { get; set; }
        public Action Action { get; set; }
    }
}