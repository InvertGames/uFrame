using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using UnityEngine;

namespace Assets.uFrameComplete.uFrame.Editor.DiagramPlugins
{
    //public class PlayMakerPlugin : DiagramPlugin
    //{
    //    public void Bla()
    //    {

    //    }

    //    public override PluginDrawer GetDrawer(ElementsDiagram diagram, PluginData data)
    //    {
    //        return new PlayMakerDiagramDrawer(data, diagram);
    //    }

    //    public override IEnumerable<IDiagramSubItem> GetItems(PluginData data)
    //    {
    //        yield break;
    //    }

    //    public override void OnAddContextItems(ElementsDiagram diagram, GenericMenu menu)
    //    {
    //        if (diagram.Data.CurrentFilter is ElementData)
    //        {
    //            menu.AddItem(new GUIContent("New Playmaker Template"), false, () =>
    //            {
    //                diagram.AddNewPluginItem(this, "NewPlaymakerTemplate");
    //                EditorWindow.GetWindow<ToolWindow>().Show(true);
    //            });
    //        }
    //    }
    //}

    //public class PlayMakerDiagramDrawer : PluginDrawer
    //{
    //    public PlayMakerDiagramDrawer(PluginData data, ElementsDiagram diagram)
    //        : base(data, diagram)
    //    {
    //    }
    //}

    public class UBehavioursViewDrawer : ViewDrawer
    {
        public UBehavioursViewDrawer()
        {
        }

        public UBehavioursViewDrawer(ViewData data, ElementsDiagram diagram)
            : base(data, diagram)
        {

        }

        private DiagramItemHeader _behavioursHeader;

        public DiagramItemHeader BehavioursHeader
        {
            get
            {
                if (_behavioursHeader != null)
                    return _behavioursHeader;


                _behavioursHeader =
                    new DiagramItemHeader() { HeaderType = typeof(UBSharedBehaviour), Label = "Behaviours" };
                _behavioursHeader.AddCommand = Container.Resolve<AddNewBehaviourCommand>();
                return _behavioursHeader;
            }
            set { _behavioursHeader = value; }
        }

        protected override IEnumerable<DiagramSubItemGroup> GetItemGroups()
        {

            var vm = Data.ViewForElement;
            if (vm == null) yield break;
            if (Data != null)
            {

                Behaviours = UBAssetManager.Behaviours.OfType<UFrameBehaviours>()
                    .Where(p => p != null && p.ViewModelTypeString == vm.ViewModelAssemblyQualifiedName).ToArray();

            }
            yield return new DiagramSubItemGroup()
            {
                Header = BehavioursHeader,
                Items = Behaviours.Select(p=>new BehaviourSubItem() { Behaviour = p}).Cast<IDiagramSubItem>().ToArray()
            };
            yield break;
        }

        public UFrameBehaviours[] Behaviours { get; set; }
    }

    public class AddNewBehaviourCommand : EditorCommand<ViewData>
    {
        public override void Perform(ViewData item)
        {
            if (!Directory.Exists(BehavioursPath))
            {
                Directory.CreateDirectory(BehavioursPath);
            }
            var asset = UBAssetManager.CreateAsset<UFrameBehaviours>(BehavioursPath, item.Name + "Behaviour");
            asset.ViewModelTypeString = item.ViewForElement.ViewModelAssemblyQualifiedName;
            
            //Save();
        }

        public override string CanPerform(ViewData item)
        {
            return null;
        }
    }
}
