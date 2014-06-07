using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Invert.uFrame.Editor.Refactoring;
using UnityEditor;
using UnityEngine;


public abstract class DiagramItem :  IDiagramItem, IRefactorable
{
    protected DiagramItem()
    {
    }

    [SerializeField]
    private bool _isCollapsed;

    [SerializeField]
    private string _name;

    [SerializeField]
    private float _x;

    [SerializeField]
    private float _y;

    [SerializeField] protected string _assemblyQualifiedName;
    private Rect _position;
    private Vector2 _location;

    [SerializeField]
    private ElementDesignerData _data;

    public virtual string InfoLabel
    {
        get
        {
            var count = AllRefactorers.Count();
            if (count == 0) return null;
            return string.Format("Refactors: {0}", count);
        }
    }

    public IEnumerable<Refactorer> AllRefactorers
    {
        get {return Refactorings.Concat(Items.OfType<IRefactorable>().SelectMany(p => p.Refactorings)); }
    }
    public Rect Position
    {
        get
        {
            return _position;
        }
        set { _position = value; }
    }

    public abstract string Label { get; }
    public bool IsSelected { get; set; }

    [SerializeField]
    private string _identifier;

    private List<Refactorer> _refactorings;

    public string Identifier{ get { return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier;}}
    public bool IsSelectable { get { return true; } }

    public virtual bool IsCollapsed
    {
        get { return _isCollapsed; }
        set
        {
            _isCollapsed = value;
            Filter.CollapsedValues[this] = value;
            Dirty = true;
        }
    }

    public bool IsEditing { get; set; }
    public abstract IEnumerable<IDiagramSubItem> Items { get; }

    public virtual string OldName
    {
        get;
        set;
    }

    public virtual string Name
    {
        get { return _name; }
        set
        {
            _name = Regex.Replace(value, "[^a-zA-Z0-9_.]+", "");
            Dirty = true;
        }
    }

    public string Highlighter { get { return null; } }
    public string FullLabel { get { return Name; } }

    public void Remove(IDiagramItem data)
    {
        Filter.Locations.Remove(this.Identifier);
    }

    public void Rename(IDiagramItem data, string name)
    {
        Rename(name);
    }

    public List<Refactorer> Refactorings
    {
        get { return _refactorings ?? (_refactorings = new List<Refactorer>()); }
        set { _refactorings = value; }
    }

    public virtual void BeginEditing()
    {
        if (RenameRefactorer == null)
        {
            RenameRefactorer = CreateRenameRefactorer();
        }
        OldName = Name;
        IsEditing = true;
    }

    public virtual RenameRefactorer CreateRenameRefactorer()
    {
        return null;
    }

    public RenameRefactorer RenameRefactorer { get; set; }

    public virtual bool ShouldRenameRefactor { get { return true; } }
    public virtual void EndEditing()
    {
        IsEditing = false;
        if (Data.DiagramItems.Count(p => p.Name == Name) > 1)
        {
            Name = OldName;
            return;
        }
        
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
    public virtual void RefactorApplied()
    {
        Refactorings.Clear();
        RenameRefactorer = null;
    }
    public Vector2[] ConnectionPoints { get; set; }

    public Vector2 Location
    {
        get
        {
            //if (Filter == this)
            //{
            //    return new Vector2((Screen.width / 2f) - (Position.width / 2f), (Screen.height / 2f) - (Position.height / 2f));
            //}
            return _location;
        }
        set
        {
            _location = value;
            Filter.Locations[this] = value;
            if (_location.x < 0)
                _location.x = 0;
            if (_location.y < 0)
                _location.y = 0;
            Dirty = true;
        }
    }


    public virtual string AssemblyQualifiedName
    {
        get
        {
            return UFrameAssetManager.DesignerVMAssemblyName.Replace("ViewModel", Name);
        }
    }

    public bool Dirty { get; set; }

    public virtual void Rename( string newName)
    {

        Name = newName;
    }

    [DiagramContextMenu("Hide",1)]
    public void RemoveFromFilter(ElementDesignerData data)
    {
        Filter.Locations.Remove(this.Identifier);
    }
    //[DiagramContextMenu("Delete From All")]
    public abstract void RemoveFromDiagram();

    [DiagramContextMenu("Rename",0)]
    public void BeginRename()
    {
       BeginEditing();
        
    }
    
    public Rect HeaderPosition { get; set; }

    public IDiagramFilter Filter
    {
        get { return Data.CurrentFilter; }
    }    

    public ElementDesignerData Data
    {
        get
        {

            return _data;
        }
        set
        {
            _data = value;
            if (value != null)
            {
                //if (!value.Locations.Keys.Contains(Identifier))
                //{
                //    value.Locations[this] = Location;
                //}
                _location = value.CurrentFilter.Locations[this];
                _isCollapsed = value.CurrentFilter.CollapsedValues[this];
                Dirty = true;
            }
        }
    }

    public abstract void CreateLink(IDiagramItem container, IDrawable target);
    public abstract bool CanCreateLink(IDrawable target);
    public abstract IEnumerable<IDiagramLink> GetLinks(IDiagramItem[] elementDesignerData);
    public abstract void RemoveLink(IDiagramItem target);
}