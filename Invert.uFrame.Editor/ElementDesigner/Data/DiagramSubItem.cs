using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Invert.uFrame.Editor.Refactoring;
using UnityEngine;

public abstract class DiagramSubItem : IDiagramSubItem
{
    [SerializeField]
    private string _name;

    [SerializeField]
    private string _identifier;

    private List<Refactorer> _refactorings;
    private bool _isSelected;

    public string OldName { get; set; }

    public string Name
    {
        get { return _name; }
        set { _name = Regex.Replace(value, "[^a-zA-Z0-9_.]+", ""); }
    }

    public abstract string Label { get; }
    public Rect Position { get; set; }

    public bool IsSelected
    {
        get { return _isSelected; }
        set
        {
            if (value == false && _isSelected)
            {
                EndEditing();
            }
            else if (value == true && !_isSelected)
            {
                BeginEditing();
            }
            _isSelected = value;
        }
    }

    public virtual void BeginEditing()
    {
        if (RenameRefactorer == null)
        {
            RenameRefactorer = CreateRenameRefactorer();
        }
        OldName = Name;
        
    }

    public RenameRefactorer RenameRefactorer { get; set; }

    public virtual RenameRefactorer CreateRenameRefactorer()
    {
        return null;
    }

    public virtual void EndEditing()
    {
        if (OldName != Name)
        {
            if (RenameRefactorer == null)
            {
                return;
            }

            RenameRefactorer.Set(this);
            if (!Refactorings.Contains(RenameRefactorer))
            {
                Refactorings.Add(RenameRefactorer);
            }
        }
    }

    public virtual string Highlighter { get { return null; } }
    public abstract string FullLabel { get; }
    public string Identifier{ get { return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier;}}
    public virtual bool IsSelectable { get { return true; } }
    public Vector2[] ConnectionPoints { get; set; }

    public List<Refactorer> Refactorings
    {
        get { return _refactorings ?? (_refactorings = new List<Refactorer>()); }
        set { _refactorings = value; }
    }

    public abstract void CreateLink(IDiagramItem container, IDrawable target);
    public abstract bool CanCreateLink(IDrawable target);
    public abstract void RemoveLink(IDiagramItem target);
    public abstract IEnumerable<IDiagramLink> GetLinks(IDiagramItem[] data);
    public abstract void Remove(IDiagramItem diagramItem);

    public virtual void Rename(IDiagramItem data, string name)
    {
        Name = name;
    }

    public void RefactorApplied()
    {
        Refactorings.Clear();
    }
}