using System.Collections.Generic;
using Invert.uFrame.Editor.Refactoring;
using UnityEngine;

public interface IDiagramItem : ISelectable,IDiagramSubItem
{
    string InfoLabel { get; }
    bool IsCollapsed { get; set; }
    bool IsEditing { get; set; }
    IEnumerable<IDiagramSubItem> Items { get; }
    //string Name { get; set; }
    Vector2 Location { get; set; }
    string AssemblyQualifiedName { get; }
    bool Dirty { get; set; }
    void Rename(string newName);
    void RemoveFromDiagram();
    Rect HeaderPosition { get; set; } 
    ElementDesignerData Data { get; set; }
    IDiagramFilter Filter { get; }
    string OldName { get; set; }
    

    void BeginEditing();
    void EndEditing();
}

public interface IRefactorable
{
    List<Refactorer> Refactorings { get;  }
    void RefactorApplied();
}