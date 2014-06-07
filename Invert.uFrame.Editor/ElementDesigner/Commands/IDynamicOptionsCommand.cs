using System;
using System.Collections.Generic;

namespace Invert.uFrame.Editor.ElementDesigner
{
    public interface IDynamicOptionsCommand
    {
        Type For { get; }
        IEnumerable<UFContextMenuItem> GetOptions(object item);
        UFContextMenuItem SelectedOption { get; set; }
        MultiOptionType OptionsType { get; }
    }
}