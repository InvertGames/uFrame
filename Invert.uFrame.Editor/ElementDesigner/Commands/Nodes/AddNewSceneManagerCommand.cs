using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddNewSceneManagerCommand : AddItemCommand<SceneManagerData>
    {
        public override string Title
        {
            get { return "Add New Scene Manager"; }
        }
        public override void Perform(ElementsDiagram item)
        {
            var data = new SceneManagerData()
            {
                Data = item.Data,
                Name = item.Data.GetUniqueName("NewSceneManager"),
                Location = new Vector2(15, 15)
            };
            item.Data.SceneManagers.Add(data);
            data.Location = item.LastMouseDownPosition;
        }
    }
}