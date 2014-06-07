using UnityEngine;

public interface IDiagramSubItem : ISelectable
{
    string Name { get; set; }
    string Highlighter { get; }
    string FullLabel { get; }
    string Identifier { get; }
    bool IsSelectable { get;}
    void Remove(IDiagramItem data);
    void Rename(IDiagramItem data, string name);


}