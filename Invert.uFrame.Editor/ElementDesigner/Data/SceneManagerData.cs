using System;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor.Refactoring;
using UnityEditor;
using UnityEngine;

[Serializable]
public class SceneManagerData : DiagramItem
{
    [SerializeField]
    private List<SceneManagerTransition> _transitions = new List<SceneManagerTransition>();

    public List<SceneManagerTransition> Transitions
    {
        get { return _transitions; }
        set { _transitions = value; }
    }

    public Type CurrentType
    {
        get
        {
            return Type.GetType(UFrameAssetManager.DesignerVMAssemblyName.Replace("ViewModel", NameAsSceneManager));
        }
    }
    public Type CurrentSettingsType
    {
        get
        {
            return Type.GetType(UFrameAssetManager.DesignerVMAssemblyName.Replace("ViewModel", NameAsSettings));
        }
    }
    public override string Label
    {
        get { return Name; }
    }

    [SerializeField] private string _subSystemIdentifier;

    public override void CreateLink(IDiagramItem container, IDrawable target)
    {
        //var scItem = target as sceneManagerData;
        //if (scItem == null) return;
        //Transitions.Add(new SceneManagerTransition() { ToIdentifier = scItem.Name,Name ="To" + scItem.Name });
    }

    public override bool CanCreateLink(IDrawable target)
    {
        return false;
    }

    [DiagramContextMenu("Add to scene", 0)]
    public void AddToScene()
    {
        var sceneManagerAssemblyName = UFrameAssetManager.DesignerVMAssemblyName.Replace("ViewModel",
            this.NameAsSceneManager);
        var type = Type.GetType(sceneManagerAssemblyName);
        if (type == null)
        {
            EditorUtility.DisplayDialog("Can't add to scene", "The diagram must be saved and have no compiler errors.",
                "OK");
            return;
        }

        GameObject obj = new GameObject("_SceneManager");
        obj.AddComponent(type);

    }
    public override IEnumerable<IDiagramLink> GetLinks(IDiagramItem[] elementDesignerData)
    {

        // Sync commands to here
        if (this.SubSystem != null)
        {
            var commands = this.SubSystem.IncludedCommands.ToArray();

            foreach (var command in commands)
            {
                var transition = Transitions.FirstOrDefault(p => p.CommandIdentifier == command.Identifier);
                if (transition == null)
                {
                    Transitions.Add(new SceneManagerTransition()
                    {
                        Name = command.Name,
                        CommandIdentifier = command.Identifier

                    });
                }
                else
                {
                    transition.Name = command.Name;
                }
            }
            Transitions.RemoveAll(p => commands.All(x => x.Identifier != p.CommandIdentifier));

        }
        

        foreach (var transition in Transitions)
        {
            
            var linkedTo = elementDesignerData.OfType<SceneManagerData>().FirstOrDefault(p => p.Identifier == transition.ToIdentifier);
            if (linkedTo == null) continue;


            yield return new TransitionLink()
            {
                From = transition,
                To = linkedTo
            };
        }
        yield break;
    }

    public override void RemoveLink(IDiagramItem target)
    {
        
    }

    public override IEnumerable<IDiagramSubItem> Items
    {
        get
        {
            //if (SubSystem == null) yield break;
            //foreach (var command in SubSystem.IncludedCommands)
            //{
            //    yield return command;
            //}
            return Transitions.Cast<IDiagramSubItem>();
        }
    }

    public override void BeginEditing()
    {
        base.BeginEditing();
        if (RenameRefactor == null)
        {
            RenameRefactor = new RenameSceneManagerRefactorer(this);
        }
    }
    public override RenameRefactorer CreateRenameRefactorer()
    {
        return new RenameSceneManagerRefactorer(this);
    }
    public override void EndEditing()
    {
    
        base.EndEditing();
    }

    public RenameSceneManagerRefactorer RenameRefactor { get; set; }

    public override void RemoveFromDiagram()
    {
        Data.SceneManagers.Remove(this);
    }

    [SerializeField]
    private FilterLocations _locations = new FilterLocations();
    public FilterLocations Locations
    {
        get { return _locations; }
        set { _locations = value; }
    }

    public bool ImportedOnly
    {
        get { return true; }
    }

    public string NameAsSceneManager
    {
        get { return string.Format("{0}", Name); }
    }

    public string NameAsSceneManagerBase
    {
        get { return string.Format("{0}Base", Name); }
        
    }

    public string SubSystemIdentifier
    {
        get { return _subSystemIdentifier; }
        set { _subSystemIdentifier = value; }
    }

    public SubSystemData SubSystem
    {
        get
        {
            return Data.SubSystems.FirstOrDefault(p => p.Identifier == SubSystemIdentifier);
        }
    }



    public string NameAsSettings
    {
        get { return string.Format("{0}Settings", Name); }
    }

    public string NameAsSettingsField { get { return string.Format("_{0}Settings", Name); } }

    public bool IsAllowed(object item, Type t)
    {
        if (t == typeof(SceneManagerData))
        {
            return false;
        }
        return true;
    }
}