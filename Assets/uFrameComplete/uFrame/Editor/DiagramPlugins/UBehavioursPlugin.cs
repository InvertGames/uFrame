using System.Collections.Generic;
using Invert.uFrame;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Data;
using UnityEditor;
using UnityEngine;

namespace Assets.uFrameComplete.uFrame.Editor.DiagramPlugins
{
    public class UBehavioursPlugin : DiagramPlugin
    {
  

        public override void Initialize(uFrameContainer container)
        {
            container.RegisterInstance(new AddNewBehaviourCommand());
            container.RegisterRelation<ViewData,IElementDrawer,UBehavioursViewDrawer>();

        }
    }
}