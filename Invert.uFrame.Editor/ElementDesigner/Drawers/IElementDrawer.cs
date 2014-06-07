using System;
using System.Collections.Generic;
using Invert.uFrame.Editor;

public interface IElementDrawer : ICommandHandler
{
    string ShouldFocus { get; }
    void Draw(ElementsDiagram elementsDiagram);
    void CalculateBounds();
    bool IsSelected { get; }
    IDiagramItem Model { get; set; }
    IEnumerable<IDiagramSubItem> Items { get; }
    ElementsDiagram Diagram { get; set; }
    Type CommandsType { get; }
    void DoubleClicked();
}