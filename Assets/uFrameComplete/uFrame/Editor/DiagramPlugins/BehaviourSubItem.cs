using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BehaviourSubItem : IDiagramSubItem
{
    private UBSharedBehaviour _Behaviour;
    private bool _isSelected;
    public Rect Position { get; set; }

    public string Label
    {
        get { return Name; }
    }

    public void CreateLink(IDiagramItem container, IDrawable target)
    {

    }

    public bool CanCreateLink(IDrawable target)
    {
        return false;
    }

    public IEnumerable<IDiagramLink> GetLinks(IDiagramItem[] elementDesignerData)
    {
        yield break;
    }

    public bool IsSelected
    {
        get { return _isSelected; }
        set
        {
            _isSelected = value;
            if (value == false)
            {
                if (!string.IsNullOrEmpty(_renameTo))
                {
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(Behaviour), Name);
                    UBAssetManager.RefreshBehaviours();
                    _renameTo = null;
                }
            }

        }
    }

    public void RemoveLink(IDiagramItem target)
    {

    }

    public Vector2[] ConnectionPoints { get; set; }

    public string Name
    {
        get
        {
            if (Behaviour == null) return "??";
            return Behaviour.name;
        }
        set
        {
            Behaviour.name = value;
            EditorUtility.SetDirty(Behaviour);
        }
    }

    public string Highlighter { get; private set; }
    public string FullLabel { get { return Name; } }
    public string Identifier { get; set; }
    public bool IsSelectable { get { return true; } }

    public UBSharedBehaviour Behaviour
    {
        get { return _Behaviour; }
        set { _Behaviour = value; }
    }

    public void Remove(IDiagramItem data)
    {
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(Behaviour));
    }

    private string _renameTo = null;
    [DiagramContextMenu("Add to/Selection", 1)]
    public void AddToSelection()
    {
        var selection = Selection.activeObject as GameObject;

        if (selection != null)
        {
            selection.AddComponent<UBComponent>().Behaviour = Behaviour;    
        }
        
        

    }
    public void Rename(IDiagramItem data, string name)
    {
        _renameTo = name;
        Behaviour.name = name;
    }
}