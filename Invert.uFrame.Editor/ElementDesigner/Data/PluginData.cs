using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Data
{
    [Serializable]
    public class PluginData : DiagramItem
    {
        [SerializeField]
        private string _pluginTypeName;

        public IDiagramPlugin Plugin { get; set; }

        public override string Label
        {
            get { return Name; }
        }

        public override void CreateLink(IDiagramItem container, IDrawable target)
        {
            
        }

        public override bool CanCreateLink(IDrawable target)
        {
            return false;
        }

        public override IEnumerable<IDiagramLink> GetLinks(IDiagramItem[] elementDesignerData)
        {
            yield break;
        }

        public override void RemoveLink(IDiagramItem target)
        {
            
        }

        public override IEnumerable<IDiagramSubItem> Items
        {
            get { yield break; }
        }

        public Type PluginType
        {
            get { return Type.GetType(_pluginTypeName); }
            set { _pluginTypeName = value.AssemblyQualifiedName; }
        }

        public string PluginTypeName
        {
            get { return _pluginTypeName; }
            set { _pluginTypeName = value; }
        }

        public override void RemoveFromDiagram()
        {
            
        }
    }

    public class PluginDrawer : DiagramItemDrawer<PluginData>
    {
        public PluginDrawer()
        {
        }

        public PluginDrawer(PluginData data, ElementsDiagram diagram) : base(data, diagram)
        {
            
        }

        protected override IEnumerable<DiagramSubItemGroup> GetItemGroups()
        {
            yield break;
        }
    }

    public interface IDiagramPlugin
    {
        void Initialize(uFrameContainer container);
    }

    public abstract class DiagramPlugin :IDiagramPlugin
    {

        public abstract void Initialize(uFrameContainer container);
    }
}
